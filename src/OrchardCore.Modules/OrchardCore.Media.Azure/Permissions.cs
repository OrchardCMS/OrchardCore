using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media.Azure;

[Feature("OrchardCore.Media.Azure.Storage")]
public class Permissions : IPermissionProvider
{
    public static readonly Permission ViewAzureMediaOptions = new("ViewAzureMediaOptions", "View Azure Media Options");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ViewAzureMediaOptions,
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
