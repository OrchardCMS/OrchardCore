using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Email.Azure;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ViewAzureEmailOptions = new("ViewAzureEmailOptions", "View Azure Email Options");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ViewAzureEmailOptions,
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
