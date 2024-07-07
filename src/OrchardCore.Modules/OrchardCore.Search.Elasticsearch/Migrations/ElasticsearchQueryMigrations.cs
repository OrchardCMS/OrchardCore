using OrchardCore.Data.Migration;
using OrchardCore.Queries.Core.Services;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Queries.Sql.Migrations;

public class ElasticsearchQueryMigrations : DataMigration
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
    public int Create()
    {
        QuerySourceHelper.MigrateQueries(ElasticQuerySource.SourceName);

        return 1;
    }
}
