using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Shortcodes;

public static class ShortcodesPermissions
{
    public static readonly Permission ManageShortcodeTemplates = new("ManageShortcodeTemplates", LocalizationSource.Create("Manage shortcode templates", typeof(ShortcodesPermissions)), isSecurityCritical: true);
}
