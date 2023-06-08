using OrchardCore.Security.Permissions;

namespace OrchardCore.Seo;

public class SeoPermissions
{
    public static readonly Permission ManageSettings = new("ManageSettings", "Manage SEO related site settings");
}
