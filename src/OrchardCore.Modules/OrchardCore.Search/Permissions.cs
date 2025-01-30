using OrchardCore.Security.Permissions;

namespace OrchardCore.Search;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        SearchPermissions.QuerySearchIndex,
        SearchPermissions.ManageSearchSettings,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'SearchPermissions.QuerySearchIndex'.")]
    public static readonly Permission QuerySearchIndex = new("QuerySearchIndex", "Query any index");

    [Obsolete("This will be removed in a future release. Instead use 'SearchPermissions.ManageSearchSettings'.")]
    public static readonly Permission ManageSearchSettings = new("ManageSearchSettings", "Manage Search Settings");

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
