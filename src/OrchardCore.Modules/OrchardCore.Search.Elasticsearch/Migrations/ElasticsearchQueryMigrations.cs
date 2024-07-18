using OrchardCore.Data.Migration;
using OrchardCore.Queries.Core;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Queries.Sql.Migrations;

public sealed class ElasticsearchQueryMigrations : DataMigration
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
    public int Create()
    {
        QueriesDocumentMigrationHelper.Migrate(ElasticQuerySource.SourceName);

        return 1;
    }
}
