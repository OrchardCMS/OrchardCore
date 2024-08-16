using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageQueries = new("ManageQueries", "Manage queries");
    public static readonly Permission ExecuteApiAll = new("ExecuteApiAll", "Execute Api - All queries", [ManageQueries]);

    private static readonly Permission _executeApi = new("ExecuteApi_{0}", "Execute Api - {0}", [ManageQueries, ExecuteApiAll]);

    private readonly IQueryManager _queryManager;
    private readonly IEnumerable<Permission> _generalPermissions =
    [
        ManageQueries,
    ];

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

        var queries = await _queryManager.ListQueriesAsync(true);

        foreach (var query in queries)
        {
            list.Add(CreatePermissionForQuery(query.Name));
        }

        return list;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _generalPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
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
