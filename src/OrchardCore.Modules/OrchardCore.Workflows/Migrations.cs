using Dapper;
using Microsoft.Extensions.DependencyInjection;
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
            .Column<string>("WorkflowStatus", c => c.WithLength(26))
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

    // This code can be removed in a later version.
    // Normalize WorkflowStatus values stored as integer strings (e.g. "5") to enum names (e.g. "Finished").
    // SQL Server and SQLite silently coerced the enum int to varchar on insert; those rows must be updated
    // so that queries comparing against enum names (introduced in migration 4) find them correctly.
    public static int UpdateFrom3()
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IStore>();
            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();
            var dialect = store.Configuration.SqlDialect;

            var tableName = $"{store.Configuration.TablePrefix}WorkflowIndex";
            var quotedTableName = dialect.QuoteForTableName(tableName, store.Configuration.Schema);
            var quotedColumnName = dialect.QuoteForColumnName("WorkflowStatus");

            var statuses = new[] { "Idle", "Starting", "Resuming", "Executing", "Halted", "Finished", "Faulted", "Aborted" };

            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();

            for (var i = 0; i < statuses.Length; i++)
            {
                var sql = $"UPDATE {quotedTableName} SET {quotedColumnName} = @name WHERE {quotedColumnName} = @id";
                await connection.ExecuteAsync(sql, new { name = statuses[i], id = i.ToString() });
            }
        });

        return 4;
    }

}
