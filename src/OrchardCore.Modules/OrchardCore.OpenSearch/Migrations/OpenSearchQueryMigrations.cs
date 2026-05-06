using OrchardCore.Data.Migration;
using OrchardCore.Queries.Core;
using OrchardCore.OpenSearch.Core.Services;

namespace OrchardCore.Queries.Sql.Migrations;

public sealed class OpenSearchQueryMigrations : DataMigration
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
    public int Create()
    {
        QueriesDocumentMigrationHelper.Migrate(OpenSearchQuerySource.SourceName);

        return 1;
    }
}
