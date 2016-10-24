using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Extensions
{
    public class ThemeExtensionInfo : IExtensionInfo
    {
        private IExtensionInfo _extensionInfo;

        public ThemeExtensionInfo(IExtensionInfo extensionInfo)
        {

            _extensionInfo = extensionInfo;
        }

        public string Id => _extensionInfo.Id;
        public IFileInfo ExtensionFileInfo => _extensionInfo.ExtensionFileInfo;
        public string SubPath => _extensionInfo.SubPath;
        public IManifestInfo Manifest => _extensionInfo.Manifest;
        public IList<IFeatureInfo> Features => _extensionInfo.Features;

        public string BaseTheme => _extensionInfo.Manifest.ConfigurationRoot["basetheme"].ToString();
    }
}
