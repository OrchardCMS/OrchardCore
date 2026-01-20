using OrchardCore.Security.Permissions;

namespace OrchardCore.Templates;

public sealed class AdminTemplatesPermissions : IPermissionProvider
{
    public static readonly Permission ManageAdminTemplates = new("ManageAdminTemplates", "Manage admin templates", isSecurityCritical: true);

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageAdminTemplates,
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
    ];
}
