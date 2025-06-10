using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing.Core;
using YesSql;

namespace OrchardCore.Indexing;

public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateTableAsync("IndexingTask", table => table
            .Column<int>("Id", col => col.PrimaryKey().Identity())
            .Column<string>("RecordId", c => c.WithLength(26))
            .Column<string>("Category", c => c.WithLength(50))
            .Column<DateTime>("CreatedUtc", col => col.NotNull())
            .Column<int>("Type")
        );

        await SchemaBuilder.AlterTableAsync("IndexingTask", table => table
            .CreateIndex("IDX_IndexingTask_RecordId_Category", "RecordId", "Category")
        );

        return 2;
    }

    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterTableAsync("IndexingTask", table => table
            .AddColumn<string>("Category", c => c.WithLength(50))
        );

        await SchemaBuilder.AlterTableAsync("IndexingTask", table => table
            .RenameColumn("ContentItemId", "RecordId")
        );

        await SchemaBuilder.AlterTableAsync("IndexingTask", table => table
            .DropIndex("IDX_IndexingTask_ContentItemId")
        );

        await SchemaBuilder.AlterTableAsync("IndexingTask", table => table
            .CreateIndex("IDX_IndexingTask_RecordId_Category", "RecordId", "Category")
        );

        ShellScope.AddDeferredTask(async scope =>
        {
            var serviceProvider = scope.ServiceProvider;

            var store = serviceProvider.GetService<IStore>();
            var dbConnectionAccessor = serviceProvider.GetService<IDbConnectionAccessor>();
            var dialect = store.Configuration.SqlDialect;

            var table = $"{store.Configuration.TablePrefix}{nameof(IndexingTask)}";
            var logger = serviceProvider.GetService<ILogger<Migrations>>();

            var quotedCategoryName = dialect.QuoteForColumnName("Category");

            var command = $"update {dialect.QuoteForTableName(table, store.Configuration.Schema)} set {quotedCategoryName} = @Category where {quotedCategoryName} is null;";

            await using var connection = dbConnectionAccessor.CreateConnection();

            try
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(command, new { Category = IndexingConstants.ContentsIndexSource });
                await connection.CloseAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while updating indexing tasks Category to Content.");

                throw;
            }
        });

        return 2;
    }
}
