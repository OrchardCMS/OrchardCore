using OrchardCore.Environment.Extensions;

namespace OrchardCore.DisplayManagement.Extensions
{
    public static class ExtensionInfoExtensions
    {
        public static bool IsTheme(this IExtensionInfo extensionInfo)
        {
            return extensionInfo is IThemeExtensionInfo;
        }
    }
}