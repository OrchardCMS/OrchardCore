using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.UrlRewriting;

public static class UrlRewritingPermissions
{
    public static readonly Permission ManageUrlRewritingRules = new Permission("ManageUrlRewritingRules", LocalizationSource.Create("Manage URLs rewriting rules", typeof(UrlRewritingPermissions)));
}
