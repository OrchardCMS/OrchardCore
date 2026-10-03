using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries;

public static class QueriesPermissions
{
    public static readonly Permission ManageSqlQueries = new("ManageSqlQueries", new LocalizationSource("Manage SQL Queries", typeof(QueriesPermissions)), true);
}
