using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries.Core;

public sealed class QueriesPermissions
{
    public static readonly Permission ManageSqlQueries = new("ManageSqlQueries", "Manage SQL Queries");
}
