using OrchardCore.Security.Permissions;

namespace OrchardCore.Shortcodes;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageShortcodeTemplates = new("ManageShortcodeTemplates", "Manage shortcode templates", isSecurityCritical: true);

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageShortcodeTemplates,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions = _allPermissions,
        },
    ];
}
