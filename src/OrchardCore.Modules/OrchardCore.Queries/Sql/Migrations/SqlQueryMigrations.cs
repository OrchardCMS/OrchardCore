using OrchardCore.Data.Migration;
using OrchardCore.Queries.Core;

namespace OrchardCore.Queries.Sql.Migrations;

public sealed class SqlQueryMigrations : DataMigration
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
    public int Create()
    {
        QuerySourceHelper.MigrateQueries(SqlQuerySource.SourceName);

        return 1;
    }
}
