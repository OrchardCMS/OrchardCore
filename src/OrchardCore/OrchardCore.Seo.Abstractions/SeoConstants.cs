using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Seo;

public static class SeoConstants
{
    public const string RobotsFileName = "robots.txt";

    public const string RobotsSettingsGroupId = "robotsSettings";

    public static readonly Permission ManageSeoSettings = new("ManageSeoSettings", LocalizedString.Create("Manage SEO related settings", typeof(SeoConstants)));
}
