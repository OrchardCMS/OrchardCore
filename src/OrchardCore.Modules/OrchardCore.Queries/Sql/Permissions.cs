using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries.Sql;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageSqlQueries = new("ManageSqlQueries", "Manage SQL Queries");

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageSqlQueries,
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
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions = _allPermissions,
        },
    ];
}
