using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries;

/// <summary>
/// Provides the permissions used to manage and execute named queries.
/// </summary>
public static class QueryPermissions
{
    /// <summary>
    /// Allows managing named queries.
    /// </summary>
    public static readonly Permission ManageQueries = new("ManageQueries", LocalizationSource.Create("Manage queries", typeof(QueryPermissions)));

    /// <summary>
    /// Allows executing all named queries through an API.
    /// </summary>
    public static readonly Permission ExecuteApiAll = new("ExecuteApiAll", LocalizationSource.Create("Execute Api - All queries", typeof(QueryPermissions)), [ManageQueries]);

    private static readonly PermissionTemplate s_executeApi = new(
        "ExecuteApi_{0}",
        LocalizationSource.Create("Execute Api - {0}", typeof(QueryPermissions)), 
        ManageQueries,
        ExecuteApiAll);

    /// <summary>
    /// Creates the permission required to execute a named query through an API.
    /// </summary>
    /// <param name="name">The query name.</param>
    /// <returns>The permission required to execute the query.</returns>
    public static Permission CreatePermissionForQuery(string name) =>
        s_executeApi.CreateDynamicPermission(name);
}
