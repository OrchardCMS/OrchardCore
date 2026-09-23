using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Shortcodes;

public static class ShortcodesPermissions
{
    public static readonly Permission ManageShortcodeTemplates = new("ManageShortcodeTemplates", LocalizedString.Create("Manage shortcode templates", typeof(ShortcodesPermissions)), isSecurityCritical: true);
}
