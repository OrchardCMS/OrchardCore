using System.Collections.Generic;
using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.DisplayManagement.Extensions
{
    public class ThemeExtensionInfo : IThemeExtensionInfo
    {
        private readonly IExtensionInfo _extensionInfo;

        public ThemeExtensionInfo(IExtensionInfo extensionInfo)
        {
            _extensionInfo = extensionInfo;

            // No need to do any further verification since the attributes themselves handle that on the front end
            var themeInfo = _extensionInfo.Manifest.ModuleInfo as ThemeAttribute;
            BaseTheme = themeInfo?.BaseTheme;
        }

        public string Id => _extensionInfo.Id;
        public string SubPath => _extensionInfo.SubPath;
        public IManifestInfo Manifest => _extensionInfo.Manifest;
        public IEnumerable<IFeatureInfo> Features => _extensionInfo.Features;
        public bool Exists => _extensionInfo.Exists;

        public string BaseTheme { get; }

        public bool HasBaseTheme()
        {
            return !string.IsNullOrWhiteSpace(BaseTheme);
        }

        public bool IsBaseThemeFeature(string featureId)
        {
            return HasBaseTheme() && featureId == BaseTheme;
        }
    }
}
