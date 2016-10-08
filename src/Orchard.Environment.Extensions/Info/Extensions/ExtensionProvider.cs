using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Info.Manifests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions.Info
{
    public class ExtensionProvider : IExtensionProvider
    {
        private IFileProvider _fileProvider;
        private IManifestBuilder _manifestBuilder;

        /// <summary>
        /// Initializes a new instance of a ExtensionProvider at the given root directory.
        /// </summary>
        /// <param name="fileProvider">fileProvider containing extensions.</param>
        /// <param name="manifestBuilder">The manifest provider.</param>
        public ExtensionProvider(
            IFileProvider fileProvider,
            IManifestBuilder manifestBuilder)
        {
            _fileProvider = fileProvider;
            _manifestBuilder = manifestBuilder;
        }

        /// <summary>
        /// Locate an extension at the given path by directly mapping path segments to physical directories.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <returns>The extension information. null returned if extension does not exist</returns>
        public IExtensionInfo GetExtensionInfo(string subPath)
        {
            var manifest = _manifestBuilder.GetManifest(subPath);
            
            if (!manifest.Exists)
            {
                return null;
            }

            var extension = _fileProvider
                .GetDirectoryContents("")
                .First(content => content.Name.Equals(subPath, StringComparison.OrdinalIgnoreCase));

            return new ExtensionInfo(extension, manifest, (ei) => {
                return BuildFeatures(ei, manifest);
            });
        }

        private IList<IFeatureInfo> BuildFeatures(
            IExtensionInfo extensionInfo,
            IManifestInfo manifestInfo)
        {
            var features = new List<IFeatureInfo>();

            // Features and Dependencies live within this section
            var featuresSection = manifestInfo.ConfigurationRoot.GetSection("features");
            if (featuresSection.Value != null)
            {
                foreach (var featureSection in featuresSection.GetChildren())
                {
                    var featureId = featureSection.Key;

                    var featureDetails = featureSection.GetChildren().ToDictionary(x => x.Key, v => v.Value);

                    var featureName = featureDetails["name"];
                    var featureDependencyIds = featureDetails["dependencies"]?
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => e.Trim())
                        .ToArray();

                    var featureInfo = new FeatureInfo(
                        featureId,
                        featureName,
                        extensionInfo,
                        featureDependencyIds);

                    features.Add(featureInfo);
                }
            }
            else
            {
                // The Extension has only one feature, itself, and that can have dependencies
                var featureId = extensionInfo.ExtensionFileInfo.Name;
                var featureName = manifestInfo.Name;

                var featureDetails = manifestInfo.ConfigurationRoot;
                var featureDependencyIds = featureDetails["dependencies"]?
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim())
                    .ToArray();

                var featureInfo = new FeatureInfo(
                    featureId,
                    featureName,
                    extensionInfo,
                    featureDependencyIds);

                features.Add(featureInfo);
            }

            return features;
        }
    }
}
