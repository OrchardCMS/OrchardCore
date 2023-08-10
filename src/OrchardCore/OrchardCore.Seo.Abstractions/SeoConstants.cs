using OrchardCore.Security.Permissions;

namespace OrchardCore.Seo;

public class SeoConstants
{
    public const string RobotsFileName = "robots.txt";

    public const string RobotsSettingsGroupId = "robotsSettings";

    public static readonly Permission ManageSeoSettings = new("ManageSeoSettings", "Manage SEO related settings");
}
