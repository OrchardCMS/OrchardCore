using OrchardCore.Data.Migration;
using OrchardCore.Queries.Core;

namespace OrchardCore.Queries.Sql.Migrations;

public sealed class SqlQueryMigrations : DataMigration, IDataMigrationWithCreate
{
    public int Create()
    {
        QueriesDocumentMigrationHelper.Migrate(SqlQuerySource.SourceName);

        return 1;
    }
}
