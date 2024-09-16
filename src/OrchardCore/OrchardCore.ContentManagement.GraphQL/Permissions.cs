using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentManagement.GraphQL;

public sealed class Permissions : IPermissionProvider
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
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
