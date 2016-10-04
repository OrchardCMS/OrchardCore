using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System;

namespace Orchard.Environment.Extensions.Info.Physical
{
    public class PhysicalExtensionInfo : IExtensionInfo
    {
        private IFileInfo _fileInfo;
        private IManifestInfo _manifestInfo;

        public PhysicalExtensionInfo(
            IFileInfo fileInfo,
            IManifestInfo manifestInfo,
            IEnumerable<IFeatureInfo> features) {

            _fileInfo = fileInfo;
            _manifestInfo = manifestInfo;
            Features = features;
        }

        public string Id => _fileInfo.Name;
        public IFileInfo Extension => _fileInfo;
        public IManifestInfo Manifest => _manifestInfo;
        public IList<IFeatureInfo> Features { get; private set; }
    }

    public interface IManifestProvider
    {
        IManifestInfo GetManifest(string subPath);
    }

    public class ExtensionProvider : IExtensionProvider
    {
        private IFileProvider _fileProvider;
        private IManifestProvider _manifestProvider;

        /// <summary>
        /// Initializes a new instance of a PhysicalExtensionProvider at the given root directory.
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

            if (manifest == null)
            {
                return null;
            }

            var extension = _fileProvider.GetFileInfo(subPath);

            if (manifest.Attributes.ContainsKey("features"))
            {
                // Features and Dependencies live within this section
            }
            else
            {
                // The Extension has only one feature, itself, and that can have dependencies

                
            }


        }
    }
}
