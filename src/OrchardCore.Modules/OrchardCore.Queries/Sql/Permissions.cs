using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries.Sql;

public class Permissions : IPermissionProvider
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
            Name = "Administrator",
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = "Editor",
            Permissions = _allPermissions,
        },
    ];
}
