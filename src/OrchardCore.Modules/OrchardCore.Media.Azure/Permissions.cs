using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media.Azure;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ViewAzureMediaOptions = new("ViewAzureMediaOptions", "View Azure Media Options");

    private static readonly IEnumerable<Permission> _allPermissions =
    [
        ViewAzureMediaOptions,
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
