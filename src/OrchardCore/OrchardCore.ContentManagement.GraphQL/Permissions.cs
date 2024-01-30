using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentManagement.GraphQL;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ApiViewContent = new("ApiViewContent", "Access view content endpoints");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ApiViewContent,
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
