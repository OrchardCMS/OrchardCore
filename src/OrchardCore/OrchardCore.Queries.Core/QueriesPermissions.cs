using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries;

public static class QueriesPermissions
{
    public static readonly Permission ManageSqlQueries = new("ManageSqlQueries", "Manage SQL Queries");
}
