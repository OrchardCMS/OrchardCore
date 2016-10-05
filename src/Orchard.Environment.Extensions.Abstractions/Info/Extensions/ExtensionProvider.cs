using Microsoft.Extensions.FileProviders;
using System.Collections;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Info
{
    public class ExtensionProvider : IExtensionProvider
    {
        private IFileProvider _fileProvider;
        private IManifestProvider _manifestProvider;

        /// <summary>
        /// Initializes a new instance of a ExtensionProvider at the given root directory.
        /// </summary>
        /// <param name="root">The root directory. This should be an absolute path.</param>
        /// <param name="manifestFileName">The manifest file name. i.e. module.txt.</param>
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

            if (manifest.Attributes.ContainsKey("features"))
            {
                // Features and Dependencies live within this section
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
