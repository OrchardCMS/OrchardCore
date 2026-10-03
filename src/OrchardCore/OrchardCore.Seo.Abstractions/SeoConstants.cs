using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Seo;

public static class SeoConstants
{
    public const string RobotsFileName = "robots.txt";

    public const string RobotsSettingsGroupId = "robotsSettings";

    public static readonly Permission ManageSeoSettings = new("ManageSeoSettings", new LocalizationSource("Manage SEO related settings", typeof(SeoConstants)));
}
