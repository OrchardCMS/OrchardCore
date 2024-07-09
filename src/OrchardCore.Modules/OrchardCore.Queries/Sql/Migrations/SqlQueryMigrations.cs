using System.Threading.Tasks;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Queries.Core;
using YesSql;

namespace OrchardCore.Queries.Sql.Migrations;

public sealed class SqlQueryMigrations : DataMigration
{
    private readonly IQueryManager _queryManager;
    private readonly IStore _store;
    private readonly IDbConnectionAccessor _dbConnectionAccessor;

    public SqlQueryMigrations(
        IQueryManager queryManager,
        IStore store,
        IDbConnectionAccessor dbConnectionAccessor)
    {
        _queryManager = queryManager;
        _store = store;
        _dbConnectionAccessor = dbConnectionAccessor;
    }

    public async Task<int> CreateAsync()
    {
        await QueriesDocumentMigrationHelper.MigrateAsync(SqlQuerySource.SourceName, _queryManager, _store, _dbConnectionAccessor);

        return 1;
    }
}
