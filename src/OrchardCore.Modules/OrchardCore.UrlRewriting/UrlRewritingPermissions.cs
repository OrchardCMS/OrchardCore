using OrchardCore.Security.Permissions;

namespace OrchardCore.UrlRewriting;

public static class UrlRewritingPermissions
{
    public static readonly Permission ManageUrlRewriting = new Permission("ManageUrlRewriting", "Manage URLs rewrites");
}
