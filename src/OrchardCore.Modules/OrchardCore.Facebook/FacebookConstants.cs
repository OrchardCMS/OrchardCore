using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Facebook;

public static class FacebookConstants
{
    public static readonly Permission ManageFacebookPixelPermission
        = new("ManageFacebookPixel", LocalizationSource.Create("Manage Facebook Pixel settings.", typeof(FacebookConstants)));

    public const string PixelSettingsGroupId = "facebook-pixel";

    public const string ConversionsApiProtectorName = "OrchardCore.Facebook.ConversionsApi";

    public static class Features
    {
        public const string Widgets = "OrchardCore.Facebook.Widgets";
        public const string Login = "OrchardCore.Facebook.Login";
        public const string Core = "OrchardCore.Facebook";
        public const string Pixel = "OrchardCore.Facebook.Pixel";
    }

    public static class SecretNames
    {
        public const string AppSecret = "Facebook.AppSecret";
    }
}
