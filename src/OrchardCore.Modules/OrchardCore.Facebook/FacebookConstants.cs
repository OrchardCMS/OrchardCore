using OrchardCore.Security.Permissions;

namespace OrchardCore.Facebook;

public static class FacebookConstants
{
    public static readonly Permission ManageFacebookPixelPermission
        = new("ManageFacebookPixel", "Manage Facebook Pixel settings.");

    public const string PixelSettingsGroupId = "facebook-pixel";

    public static class Features
    {
        public const string Widgets = "OrchardCore.Facebook.Widgets";
        public const string Login = "OrchardCore.Facebook.Login";
        public const string Core = "OrchardCore.Facebook";
        public const string Pixel = "OrchardCore.Facebook.Pixel";
    }
}
