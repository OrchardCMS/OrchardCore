using OrchardCore.Security.Permissions;

namespace OrchardCore.Sitemaps;

public sealed class SitemapsPermissions
{
    public static readonly Permission ManageSitemaps = new("ManageSitemaps", "Manage sitemaps");
}
