using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Workflows.Indexes;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Workflows;

public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<WorkflowTypeIndex>(table => table
            .Column<string>("WorkflowTypeId", c => c.WithLength(26))
            .Column<string>("Name")
            .Column<bool>("IsEnabled")
            .Column<bool>("HasStart")
        );

        await SchemaBuilder.AlterIndexTableAsync<WorkflowTypeIndex>(table => table
            .CreateIndex("IDX_WorkflowTypeIndex_DocumentId",
                "DocumentId",
                "WorkflowTypeId",
                "Name",
                "IsEnabled",
                "HasStart")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<WorkflowTypeStartActivitiesIndex>(table => table
            .Column<string>("WorkflowTypeId")
            .Column<string>("Name")
            .Column<bool>("IsEnabled")
            .Column<string>("StartActivityId")
            .Column<string>("StartActivityName")
        );

        await SchemaBuilder.AlterIndexTableAsync<WorkflowTypeStartActivitiesIndex>(table => table
            .CreateIndex("IDX_WorkflowTypeStartActivitiesIndex_DocumentId",
                "DocumentId",
                "WorkflowTypeId",
                "StartActivityId",
                "StartActivityName",
                "IsEnabled")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<WorkflowIndex>(table => table
            .Column<string>("WorkflowTypeId", c => c.WithLength(26))
            .Column<string>("WorkflowId", c => c.WithLength(26))
            .Column<int>("WorkflowStatus")
            .Column<DateTime>("CreatedUtc")
        );

        await SchemaBuilder.AlterIndexTableAsync<WorkflowIndex>(table => table
            .CreateIndex("IDX_WorkflowIndex_DocumentId",
                "DocumentId",
                "WorkflowTypeId",
                "WorkflowId",
                "WorkflowStatus",
                "CreatedUtc")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<WorkflowBlockingActivitiesIndex>(table => table
            .Column<string>("ActivityId")
            .Column<string>("ActivityName")
            .Column<bool>("ActivityIsStart")
            .Column<string>("WorkflowTypeId")
            .Column<string>("WorkflowId")
            .Column<string>("WorkflowCorrelationId")
        );

        await SchemaBuilder.AlterIndexTableAsync<WorkflowBlockingActivitiesIndex>(table => table
            .CreateIndex("IDX_WFBlockingActivities_DocumentId_ActivityId",
                "DocumentId",
                "ActivityId",
                "WorkflowTypeId",
                "WorkflowId")
        );

        await SchemaBuilder.AlterIndexTableAsync<WorkflowBlockingActivitiesIndex>(table => table
            .CreateIndex("IDX_WFBlockingActivities_DocumentId_ActivityName",
                "DocumentId",
                "ActivityName",
                "WorkflowTypeId",
                "WorkflowCorrelationId")
        );

        // Shortcut other migration steps on new content definition schemas.
        return 4;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom3Async()
    {
        // Add a temporary integer column to hold the migrated WorkflowStatus values.
        await SchemaBuilder.AlterIndexTableAsync<WorkflowIndex>(table => table
            .AddColumn<int>("WorkflowStatusTemp", c => c.WithDefault(0))
        );

        ShellScope.AddDeferredTask(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IStore>();
            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Migrations>>();

            var dialect = store.Configuration.SqlDialect;
            var tableName = store.Configuration.TableNameConvention.GetIndexTable(typeof(WorkflowIndex), null);
            var quotedTableName = dialect.QuoteForTableName($"{store.Configuration.TablePrefix}{tableName}", store.Configuration.Schema);
            var quotedOldColumn = dialect.QuoteForColumnName("WorkflowStatus");
            var quotedNewColumn = dialect.QuoteForColumnName("WorkflowStatusTemp");

            // The CASE statement handles data stored as either an integer string ("0", "1", ...)
            // or as the enum member name ("Idle", "Starting", ...).
            var updateSql =
                $"""
                UPDATE {quotedTableName}
                SET {quotedNewColumn} = CASE
                    WHEN {quotedOldColumn} IN ('0', 'Idle') THEN 0
                    WHEN {quotedOldColumn} IN ('1', 'Starting') THEN 1
                    WHEN {quotedOldColumn} IN ('2', 'Resuming') THEN 2
                    WHEN {quotedOldColumn} IN ('3', 'Executing') THEN 3
                    WHEN {quotedOldColumn} IN ('4', 'Halted') THEN 4
                    WHEN {quotedOldColumn} IN ('5', 'Finished') THEN 5
                    WHEN {quotedOldColumn} IN ('6', 'Faulted') THEN 6
                    WHEN {quotedOldColumn} IN ('7', 'Aborted') THEN 7
                    ELSE 0
                END
                """;

            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync(store.Configuration.IsolationLevel);
            var schemaBuilder = new SchemaBuilder(store.Configuration, transaction);

            try
            {
                // Migrate data from the old string column to the new integer column.
                await transaction.Connection.ExecuteAsync(updateSql, null, transaction);

                // Drop the index first; SQL Server requires this before a referenced column can be dropped.
                await schemaBuilder.AlterIndexTableAsync<WorkflowIndex>(table => table
                    .DropIndex("IDX_WorkflowIndex_DocumentId")
                );

                // Drop the old string column.
                await schemaBuilder.AlterIndexTableAsync<WorkflowIndex>(table => table
                    .DropColumn("WorkflowStatus")
                );

                // Rename the temporary integer column to WorkflowStatus.
                await schemaBuilder.AlterIndexTableAsync<WorkflowIndex>(table => table
                    .RenameColumn("WorkflowStatusTemp", "WorkflowStatus")
                );

                // Recreate the index on the new integer column.
                await schemaBuilder.AlterIndexTableAsync<WorkflowIndex>(table => table
                    .CreateIndex("IDX_WorkflowIndex_DocumentId",
                        "DocumentId",
                        "WorkflowTypeId",
                        "WorkflowId",
                        "WorkflowStatus",
                        "CreatedUtc")
                );

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "An error occurred while migrating the 'WorkflowStatus' column to an integer type.");

                throw;
            }
        });

        return 4;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<WorkflowIndex>(table =>
        {
            table.AddColumn<string>("WorkflowStatus");
        });

        return 2;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<WorkflowTypeIndex>(table => table
            .CreateIndex("IDX_WorkflowTypeIndex_DocumentId",
                "DocumentId",
                "WorkflowTypeId",
                "Name",
                "IsEnabled",
                "HasStart")
        );

        await SchemaBuilder.AlterIndexTableAsync<WorkflowTypeStartActivitiesIndex>(table => table
            .CreateIndex("IDX_WorkflowTypeStartActivitiesIndex_DocumentId",
                "DocumentId",
                "WorkflowTypeId",
                "StartActivityId",
                "StartActivityName",
                "IsEnabled")
        );

        await SchemaBuilder.AlterIndexTableAsync<WorkflowIndex>(table => table
            .CreateIndex("IDX_WorkflowIndex_DocumentId",
                "DocumentId",
                "WorkflowTypeId",
                "WorkflowId",
                "WorkflowStatus",
                "CreatedUtc")
        );

        await SchemaBuilder.AlterIndexTableAsync<WorkflowBlockingActivitiesIndex>(table => table
            .CreateIndex("IDX_WFBlockingActivities_DocumentId_ActivityId",
                "DocumentId",
                "ActivityId",
                "WorkflowTypeId",
                "WorkflowId")
        );

        await SchemaBuilder.AlterIndexTableAsync<WorkflowBlockingActivitiesIndex>(table => table
            .CreateIndex("IDX_WFBlockingActivities_DocumentId_ActivityName",
                "DocumentId",
                "ActivityName",
                "WorkflowTypeId",
                "WorkflowCorrelationId")
        );

        return 3;
    }
}
