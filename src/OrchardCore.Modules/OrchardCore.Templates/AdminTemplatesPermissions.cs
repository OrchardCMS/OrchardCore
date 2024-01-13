using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Templates;

public class AdminTemplatesPermissions : IPermissionProvider
{
    public static readonly Permission ManageAdminTemplates = new("ManageAdminTemplates", "Manage admin templates", isSecurityCritical: true);

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _allStereotypes;

    private readonly static IEnumerable<PermissionStereotype> _allStereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        ManageAdminTemplates,
    ];
}
