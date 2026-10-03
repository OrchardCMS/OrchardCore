using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Sitemaps;

public static class SitemapsPermissions
{
    public static readonly Permission ManageSitemaps = new("ManageSitemaps", LocalizationSource.Create("Manage sitemaps", typeof(SitemapsPermissions)));
}
