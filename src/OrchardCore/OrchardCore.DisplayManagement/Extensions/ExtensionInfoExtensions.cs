using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.DisplayManagement.Extensions
{
    public static class ExtensionInfoExtensions
    {
        public static bool IsTheme(this IExtensionInfo extensionInfo) =>
            extensionInfo is IThemeExtensionInfo;

        public static bool IsTheme(this IFeatureInfo featureInfo) =>
            featureInfo.Extension is IThemeExtensionInfo &&
            featureInfo.Id == featureInfo.Extension.Id;
    }
}
