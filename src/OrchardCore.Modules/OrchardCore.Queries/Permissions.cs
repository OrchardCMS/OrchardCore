using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageQueries = new("ManageQueries", "Manage queries");
    public static readonly Permission ExecuteApiAll = new("ExecuteApiAll", "Execute Api - All queries", new[] { ManageQueries });

    private static readonly Permission _executeApi = new("ExecuteApi_{0}", "Execute Api - {0}", new[] { ManageQueries, ExecuteApiAll });

    private readonly IEnumerable<Permission> _generalPermissions =
    [
        ManageQueries,
    ];

    private readonly IQueryManager _queryManager;

    public Permissions(IQueryManager queryManager)
    {
        _queryManager = queryManager;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var list = new List<Permission>()
        {
            ManageQueries,
            ExecuteApiAll,
        };

        foreach (var query in await _queryManager.ListQueriesAsync())
        {
            list.Add(CreatePermissionForQuery(query.Name));
        }

        return list;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = "Administrator",
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = "Editor",
            Permissions = _generalPermissions,
        },
    ];

    public static Permission CreatePermissionForQuery(string name)
        => new(
                string.Format(_executeApi.Name, name),
                string.Format(_executeApi.Description, name),
                _executeApi.ImpliedBy
            );
}
