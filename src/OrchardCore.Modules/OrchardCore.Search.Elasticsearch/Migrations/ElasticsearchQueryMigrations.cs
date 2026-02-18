using OrchardCore.Data.Migration;
using OrchardCore.Queries.Core;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Queries.Sql.Migrations;

public sealed class ElasticsearchQueryMigrations : DataMigration, IDataMigrationWithCreate
{
    public int Create()
    {
        QueriesDocumentMigrationHelper.Migrate(ElasticsearchQuerySource.SourceName);

        return 1;
    }
}
