using System.Collections.Generic;
using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.DisplayManagement.Extensions
{
    public interface IThemeExtensionInfo : IExtensionInfo { }

    public class ThemeExtensionInfo : IThemeExtensionInfo
    {
        private readonly IExtensionInfo _extensionInfo;

        public ThemeExtensionInfo(IExtensionInfo extensionInfo)
        {
            _extensionInfo = extensionInfo;

            var themeInfo = _extensionInfo.Manifest.ModuleInfo as ThemeAttribute;
            var baseTheme = themeInfo?.BaseTheme;

            if (baseTheme != null && baseTheme.Length != 0)
            {
                BaseTheme = baseTheme.Trim().ToString();
            }
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
