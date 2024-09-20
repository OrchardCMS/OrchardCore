using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries;

public sealed class QueriesPermissions
{
    public static readonly Permission ManageSqlQueries = new("ManageSqlQueries", "Manage SQL Queries");
}
