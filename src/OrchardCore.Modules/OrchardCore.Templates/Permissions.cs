using OrchardCore.Security.Permissions;

namespace OrchardCore.Templates;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageTemplates = new("ManageTemplates", "Manage templates", isSecurityCritical: true);

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageTemplates,
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
