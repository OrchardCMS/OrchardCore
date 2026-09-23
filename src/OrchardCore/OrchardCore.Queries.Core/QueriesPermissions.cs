using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries;

public static class QueriesPermissions
{
    public static readonly Permission ManageSqlQueries = new("ManageSqlQueries", LocalizedString.Create("Manage SQL Queries", typeof(QueriesPermissions)), true);
}
