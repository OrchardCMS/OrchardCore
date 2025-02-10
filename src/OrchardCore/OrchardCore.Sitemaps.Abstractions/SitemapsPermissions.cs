using OrchardCore.Security.Permissions;

namespace OrchardCore.Sitemaps;

public static class SitemapsPermissions
{
    public static readonly Permission ManageSitemaps = new("ManageSitemaps", "Manage sitemaps");
}
