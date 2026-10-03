using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Templates;

public sealed class AdminTemplatesPermissions : IPermissionProvider
{
    public static readonly Permission ManageAdminTemplates = new("ManageAdminTemplates", LocalizationSource.Create("Manage admin templates", typeof(AdminTemplatesPermissions)), isSecurityCritical: true);

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
