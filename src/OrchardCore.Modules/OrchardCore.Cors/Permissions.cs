using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Cors;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageCorsSettings = new("ManageCorsSettings", "Managing Cors Settings", isSecurityCritical: true);


    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ManageCorsSettings,
    ];

    private readonly static IEnumerable<PermissionStereotype> _stereotypes =
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
