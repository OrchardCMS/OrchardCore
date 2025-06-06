using OrchardCore.Indexing.Core;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search;

public sealed class Permissions : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        SearchPermissions.ManageSearchSettings,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'IndexingPermissions.QuerySearchIndex'.")]
    public static readonly Permission QuerySearchIndex = IndexingPermissions.QuerySearchIndex;

    [Obsolete("This will be removed in a future release. Instead use 'SearchPermissions.ManageSearchSettings'.")]
    public static readonly Permission ManageSearchSettings = SearchPermissions.ManageSearchSettings;

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
