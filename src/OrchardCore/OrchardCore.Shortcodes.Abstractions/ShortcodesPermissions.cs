using OrchardCore.Security.Permissions;

namespace OrchardCore.Shortcodes;

public sealed class ShortcodesPermissions
{
    public static readonly Permission ManageShortcodeTemplates = new("ManageShortcodeTemplates", "Manage shortcode templates", isSecurityCritical: true);

}
