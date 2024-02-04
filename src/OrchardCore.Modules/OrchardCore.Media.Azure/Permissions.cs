using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media.Azure;

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
            Name = "Administrator",
            Permissions = _allPermissions,
        },
    ];
}
