using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageQueries = new("ManageQueries", "Manage queries");
    public static readonly Permission ExecuteApiAll = new("ExecuteApiAll", "Execute Api - All queries", new[] { ManageQueries });

    private static readonly Permission _executeApi = new("ExecuteApi_{0}", "Execute Api - {0}", new[] { ManageQueries, ExecuteApiAll });

    private readonly IQueryManager _queryManager;

    public Permissions(IQueryManager queryManager)
    {
        _queryManager = queryManager;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var list = new List<Permission>(_allPermissions);

        foreach (var query in await _queryManager.ListQueriesAsync())
        {
            list.Add(CreatePermissionForQuery(query.Name));
        }

        return list;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        => _allStereotypes;

    public static Permission CreatePermissionForQuery(string name)
        => new(
                string.Format(_executeApi.Name, name),
                string.Format(_executeApi.Description, name),
                _executeApi.ImpliedBy
            );

    private readonly static IEnumerable<PermissionStereotype> _allStereotypes =
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

    private readonly static IEnumerable<Permission> _allPermissions =
    [
        ManageQueries,
        ExecuteApiAll,
    ];
}
