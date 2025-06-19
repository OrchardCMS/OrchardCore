using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing.Core;
using YesSql;

namespace OrchardCore.Indexing;

[Obsolete("This migration will be removed in a future release, but must remain until all sites have been successfully migrated to Orchard Core 3.0.")]
public sealed class Migrations : DataMigration
{
#pragma warning disable CA1822 // Mark members as static
    public int Create()
    {
        // This migration originally created the 'IndexingTask' table before Orchard Core 3.
        // That table have since been deprecated and replaced by the new 'RecordIndexingTask' table.
        // When this migration runs for the first time, no action is required.
        // It is retained to allow migration of existing data from the old 'IndexingTask' table to the new 'RecordIndexingTask' table.
        // This migration will be removed in a future version of Orchard Core.

        return 5;
    }

    public int UpdateFrom1()
    {
        return 2;
    }

    public int UpdateFrom2()
    {
        return 3;
    }

    public int UpdateFrom3()
    {
        return 4;
    }
#pragma warning restore CA1822 // Mark members as static

    public int UpdateFrom4()
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            // This logic must be deferred to ensure that other migrations create the necessary database tables first.

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

                await connection.ExecuteAsync($"drop table {indexingTaskTable}");
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
