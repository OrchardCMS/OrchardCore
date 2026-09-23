using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Sitemaps;

public static class SitemapsPermissions
{
    public static readonly Permission ManageSitemaps = new("ManageSitemaps", LocalizedString.Create("Manage sitemaps", typeof(SitemapsPermissions)));
}
