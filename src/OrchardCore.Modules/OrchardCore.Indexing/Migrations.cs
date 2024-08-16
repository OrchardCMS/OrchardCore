using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Indexing;

public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateTableAsync(nameof(IndexingTask), table => table
            .Column<int>("Id", col => col.PrimaryKey().Identity())
            .Column<string>("ContentItemId", c => c.WithLength(26))
            .Column<DateTime>("CreatedUtc", col => col.NotNull())
            .Column<int>("Type")
        );

        await SchemaBuilder.AlterTableAsync(nameof(IndexingTask), table => table
            .CreateIndex("IDX_IndexingTask_ContentItemId", "ContentItemId")
        );

        PopulateIndexingTaskTableWithContentItems();

        return 2;
    }

#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom1()
#pragma warning restore CA1822 // Mark members as static
    {
        PopulateIndexingTaskTableWithContentItems();
        return 2;
    }

    private void PopulateIndexingTaskTableWithContentItems()
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var clock = scope.ServiceProvider.GetRequiredService<IClock>();
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Migrations>>();
            var tablePrefix = session.Store.Configuration.TablePrefix;

            logger.LogDebug("Updating IndexingTask records");

            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();
            var dialect = session.Store.Configuration.SqlDialect;

            try
            {
                var schema = session.Store.Configuration.Schema;
                var indexingTableName = dialect.QuoteForTableName($"{tablePrefix}{nameof(IndexingTask)}", schema);

                var contentItemIdColumnName = dialect.QuoteForColumnName(nameof(IndexingTask.ContentItemId));
                var createdUtcColumnName = dialect.QuoteForColumnName(nameof(IndexingTask.CreatedUtc));
                var typeColumnName = dialect.QuoteForColumnName(nameof(IndexingTask.Type));

                var indexingSelectorBuildr = dialect.CreateBuilder(tablePrefix);
                indexingSelectorBuildr.Select();
                indexingSelectorBuildr.Selector(nameof(IndexingTask), nameof(IndexingTask.ContentItemId), schema);
                indexingSelectorBuildr.Table(nameof(IndexingTask), null, schema);

                var sqlBuilder = dialect.CreateBuilder(tablePrefix);
                sqlBuilder.Select();
                sqlBuilder.Selector(nameof(ContentItemIndex), nameof(ContentItemIndex.ContentItemId), schema);
                sqlBuilder.AddSelector(", @CreatedUtc, 0");
                sqlBuilder.Table(nameof(ContentItemIndex), null, schema);

                sqlBuilder.WhereAnd($"{dialect.QuoteForColumnName(nameof(ContentItemIndex.Latest))} = 1");
                sqlBuilder.WhereAnd($"{dialect.QuoteForColumnName(nameof(ContentItemIndex.ContentItemId))} {dialect.NotInSelectOperator(indexingSelectorBuildr.ToSqlString())}");

                var updateCmd =
                    $"INSERT INTO {indexingTableName} ({contentItemIdColumnName}, {createdUtcColumnName},  {typeColumnName})" +
                    sqlBuilder.ToSqlString();

                await connection.ExecuteAsync(updateCmd, new { CreatedUtc = clock.UtcNow });
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while updating IndexingTask records");
                throw;
            }
        });
    }
}
