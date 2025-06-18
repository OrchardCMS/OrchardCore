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
        await SchemaBuilder.CreateTableAsync("RecordIndexingTask", table => table
           .Column<int>("Id", col => col.PrimaryKey().Identity())
           .Column<string>("RecordId", c => c.WithLength(26))
           .Column<string>("Category", c => c.WithLength(50))
           .Column<DateTime>("CreatedUtc", col => col.NotNull())
           .Column<int>("Type")
       );

        await SchemaBuilder.AlterTableAsync("RecordIndexingTask", table => table
            .CreateIndex("IDX_RecordIndexingTask_RecordId_Category", "RecordId", "Category")
        );

        return 5;
    }

#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom1()
#pragma warning restore CA1822 // Mark members as static
    {
        return 2;
    }

#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom2()
#pragma warning restore CA1822 // Mark members as static
    {
        return 3;
    }

#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom3()
#pragma warning restore CA1822 // Mark members as static
    {
        return 4;
    }

    public async Task<int> UpdateFrom4Async()
    {
        await SchemaBuilder.CreateTableAsync("RecordIndexingTask", table => table
           .Column<int>("Id", col => col.PrimaryKey().Identity())
           .Column<string>("RecordId", c => c.WithLength(26))
           .Column<string>("Category", c => c.WithLength(50))
           .Column<DateTime>("CreatedUtc", col => col.NotNull())
           .Column<int>("Type")
       );

        await SchemaBuilder.AlterTableAsync("RecordIndexingTask", table => table
            .CreateIndex("IDX_RecordIndexingTask_RecordId_Category", "RecordId", "Category")
        );

        ShellScope.AddDeferredTask(async scope =>
        {
            var serviceProvider = scope.ServiceProvider;

            var store = serviceProvider.GetService<IStore>();
            var dbConnectionAccessor = serviceProvider.GetService<IDbConnectionAccessor>();
            var dialect = store.Configuration.SqlDialect;

            var recordIndexingTaskTable = $"{store.Configuration.TablePrefix}{nameof(RecordIndexingTask)}";
            var logger = serviceProvider.GetService<ILogger<Migrations>>();

            var quotedRecordIdName = dialect.QuoteForColumnName("RecordId");
            var quotedCategoryName = dialect.QuoteForColumnName("Category");
            var quotedCreatedUtcName = dialect.QuoteForColumnName("CreatedUtc");
            var quotedTypeName = dialect.QuoteForColumnName("Type");
            var quotedContentItemIdName = dialect.QuoteForColumnName("ContentItemId");
            var quotedIdName = dialect.QuoteForColumnName("Id");

            var indexingTaskTable = $"{store.Configuration.TablePrefix}IndexingTask";

            var originalTableQuery =
                $"""
                insert into {recordIndexingTaskTable} ({quotedRecordIdName}, {quotedCategoryName}, {quotedCreatedUtcName}, {quotedTypeName})
                select {quotedContentItemIdName}, @Category, {quotedCreatedUtcName}, {quotedTypeName} from {indexingTaskTable} order by {quotedIdName} 
                """;

            await using var connection = dbConnectionAccessor.CreateConnection();

            try
            {
                await connection.OpenAsync();
                try
                {
                    await connection.ExecuteAsync(originalTableQuery, new
                    {
                        Category = IndexingConstants.ContentsIndexSource,
                    });
                }
                catch
                {
                    // In the preview version, we attempted to rename the 'ContentItemId' column to 'RecordId' and add a 'Category' column to the 'IndexingTask' table.
                    // However, for large tables, this could lead to timeouts. To avoid this, we opted to create a new table called 'RecordIndexingTask' and migrate the data.
                    // If the previous SQL statement (in the try block) throws an exception, it's likely the renaming already succeeded for this tenant.
                    // In that case, we assume the new column names exist and use them when populating the new table.
                    var previewTableQuery =
                    $"""
                    insert into {recordIndexingTaskTable} ({quotedRecordIdName}, {quotedCategoryName}, {quotedCreatedUtcName}, {quotedTypeName})
                    select {quotedRecordIdName}, {quotedCategoryName}, {quotedCreatedUtcName}, {quotedTypeName} from {indexingTaskTable} order by {quotedIdName} 
                    """;

                    await connection.ExecuteAsync(previewTableQuery);
                }

                await connection.CloseAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while updating indexing tasks Category to Content.");

                throw;
            }
        });

        return 5;
    }
}
