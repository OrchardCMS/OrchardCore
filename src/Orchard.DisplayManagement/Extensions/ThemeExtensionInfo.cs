using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;

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
                _baseTheme = baseTheme.ToString();
            }
        }

        public string Id => _extensionInfo.Id;
        public IFileInfo ExtensionFileInfo => _extensionInfo.ExtensionFileInfo;
        public string SubPath => _extensionInfo.SubPath;
        public IManifestInfo Manifest => _extensionInfo.Manifest;
        public IFeatureInfoList Features => _extensionInfo.Features;

        public string BaseTheme => _baseTheme;

        public bool HasBaseTheme() {
            return _baseTheme != null;
        }

        public bool IsBaseThemeFeature(string featureName) {
            return featureName == _baseTheme;
        }
    }
}
