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

        private readonly string[] _baseThemes;

        public ThemeExtensionInfo(IExtensionInfo extensionInfo)
        {
            _extensionInfo = extensionInfo;
            
            var baseThemes = _extensionInfo.Manifest.ConfigurationRoot["basethemes"];

            if (baseThemes != null && baseThemes.Length != 0) {
                _baseThemes = baseThemes.ToString()
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => e.Trim())
                        .ToArray();
            }
        }

        public string Id => _extensionInfo.Id;
        public IFileInfo ExtensionFileInfo => _extensionInfo.ExtensionFileInfo;
        public string SubPath => _extensionInfo.SubPath;
        public IManifestInfo Manifest => _extensionInfo.Manifest;
        public IEnumerable<IFeatureInfo> Features => _extensionInfo.Features;
        public bool Exists => _extensionInfo.Exists;

        public IEnumerable<string> BaseThemes => _baseThemes;

        public bool HasBaseThemes() {
            return _baseThemes != null && _baseThemes.Length > 0;
        }

        public bool IsBaseThemeFeature(string featureId) {
            return HasBaseThemes() && _baseThemes.Contains(featureId);
        }
    }
}
