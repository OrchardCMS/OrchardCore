using OrchardCore.Data.Migration;
using OrchardCore.Queries.Core;
using OrchardCore.Search.Lucene;

namespace OrchardCore.Queries.Sql.Migrations;

public sealed class LuceneQueryMigrations : DataMigration
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
    public int Create()
    {
        QueriesDocumentMigrationHelper.Migrate(LuceneQuerySource.SourceName);

        return 1;
    }
}
