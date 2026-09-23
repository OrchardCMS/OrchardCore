using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.UrlRewriting;

public static class UrlRewritingPermissions
{
    public static readonly Permission ManageUrlRewritingRules = new Permission("ManageUrlRewritingRules", LocalizedString.Create("Manage URLs rewriting rules", typeof(UrlRewritingPermissions)));
}
