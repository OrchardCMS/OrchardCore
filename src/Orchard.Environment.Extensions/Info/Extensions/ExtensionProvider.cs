using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions.Info
{
    public class ExtensionProvider : IExtensionProvider
    {
        private IFileProvider _fileProvider;
        private IManifestProvider _manifestProvider;

        /// <summary>
        /// Initializes a new instance of a ExtensionProvider at the given root directory.
        /// </summary>
        /// <param name="fileProvider">fileProvider containing extensions.</param>
        /// <param name="manifestProvider">The manifest provider.</param>
        public ExtensionProvider(IFileProvider fileProvider, IManifestProvider manifestProvider)
        {
            _fileProvider = fileProvider;
            _manifestProvider = manifestProvider;
        }

        /// <summary>
        /// Locate an extension at the given path by directly mapping path segments to physical directories.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <returns>The extension information. null returned if extension does not exist</returns>
        public IExtensionInfo GetExtensionInfo(string subPath)
        {
            var manifest = _manifestProvider.GetManifest(subPath);
            
            if (!manifest.Exists)
            {
                return null;
            }

            var extension = _fileProvider.GetFileInfo(subPath);

            // This check man have already been done when checking for manifest
            if (!extension.Exists)
            {
                return null;
            }

            var features = new List<IFeatureInfo>();

            // Features and Dependencies live within this section
            var featuresSection = manifest.ConfigurationRoot.GetSection("features");
            if (featuresSection != null)
            {
                foreach (var featureSection in featuresSection.GetChildren())
                {
                    var featureId = featureSection.Key;

                    var featureDetails = featureSection.GetChildren().ToDictionary(x => x.Key, v => v.Value);

                    var featureName = featureDetails["name"];
                    var featureDependencyExtensionIds = featureDetails["dependencies"]
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => e.Trim())
                        .ToArray();

                    //var featureInfo = new FeatureInfo(
                    //    featureId,
                    //    featureName,
                    //    null,
                    //    new List<IFeatureInfo>());
                }
            }
            else
            {
                // The Extension has only one feature, itself, and that can have dependencies
            }

            return new ExtensionInfo(
                extension,
                manifest,
                features);
        }
    }
}
