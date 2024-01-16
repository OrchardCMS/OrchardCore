using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Templates;

public class AdminTemplatesPermissions : IPermissionProvider
{
    public static readonly Permission ManageAdminTemplates = new("ManageAdminTemplates", "Manage admin templates", isSecurityCritical: true);

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageAdminTemplates,
    ];

    private static readonly IEnumerable<PermissionStereotype> _stereotypes =
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _stereotypes;
}
