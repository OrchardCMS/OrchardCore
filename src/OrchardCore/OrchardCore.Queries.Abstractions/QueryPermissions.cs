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
    public static readonly Permission ManageQueries = new("ManageQueries", "Manage queries");

    /// <summary>
    /// Allows executing all named queries through an API.
    /// </summary>
    public static readonly Permission ExecuteApiAll = new("ExecuteApiAll", "Execute Api - All queries", [ManageQueries]);

    private static readonly Permission s_executeApi = new("ExecuteApi_{0}", "Execute Api - {0}", [ManageQueries, ExecuteApiAll]);

    /// <summary>
    /// Creates the permission required to execute a named query through an API.
    /// </summary>
    /// <param name="name">The query name.</param>
    /// <returns>The permission required to execute the query.</returns>
    public static Permission CreatePermissionForQuery(string name)
        => new(
            string.Format(s_executeApi.Name, name),
            string.Format(s_executeApi.Description, name),
            s_executeApi.ImpliedBy
        );
}
