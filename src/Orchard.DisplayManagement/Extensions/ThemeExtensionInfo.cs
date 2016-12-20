using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Extensions
{
    public class ThemeExtensionInfo : IExtensionInfo
    {
        private readonly IExtensionInfo _extensionInfo;

        private readonly string _baseTheme;

        public ThemeExtensionInfo(IExtensionInfo extensionInfo)
        {
            _extensionInfo = extensionInfo;
            
            var baseTheme = _extensionInfo.Manifest.ConfigurationRoot["basetheme"];

            if (baseTheme != null && baseTheme.Length != 0) {
                _baseTheme = baseTheme.Trim().ToString();
            }
        }

        public string Id => _extensionInfo.Id;
        public IFileInfo ExtensionFileInfo => _extensionInfo.ExtensionFileInfo;
        public string SubPath => _extensionInfo.SubPath;
        public IManifestInfo Manifest => _extensionInfo.Manifest;
        public IEnumerable<IFeatureInfo> Features => _extensionInfo.Features;
        public bool Exists => _extensionInfo.Exists;

        public string BaseTheme => _baseTheme;

        public bool HasBaseTheme() {
            return !string.IsNullOrWhiteSpace(_baseTheme);
        }

        public bool IsBaseThemeFeature(string featureId) {
            return HasBaseTheme() && featureId == _baseTheme;
        }
    }
}
