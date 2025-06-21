using OrchardCore.Data.Migration;
using OrchardCore.Workflows.Indexes;
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
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<WorkflowTypeIndex>(table => table
            .CreateIndex("IDX_WorkflowTypeIndex_DocumentId",
                "DocumentId",
                "WorkflowTypeId",
                "Name",
                "IsEnabled",
                "HasStart")
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateMapIndexTableAsync<WorkflowTypeStartActivitiesIndex>(table => table
            .Column<string>("WorkflowTypeId")
            .Column<string>("Name")
            .Column<bool>("IsEnabled")
            .Column<string>("StartActivityId")
            .Column<string>("StartActivityName")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<WorkflowTypeStartActivitiesIndex>(table => table
            .CreateIndex("IDX_WorkflowTypeStartActivitiesIndex_DocumentId",
                "DocumentId",
                "WorkflowTypeId",
                "StartActivityId",
                "StartActivityName",
                "IsEnabled")
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateMapIndexTableAsync<WorkflowIndex>(table => table
            .Column<string>("WorkflowTypeId", c => c.WithLength(26))
            .Column<string>("WorkflowId", c => c.WithLength(26))
            .Column<string>("WorkflowStatus", c => c.WithLength(26))
            .Column<DateTime>("CreatedUtc")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<WorkflowIndex>(table => table
            .CreateIndex("IDX_WorkflowIndex_DocumentId",
                "DocumentId",
                "WorkflowTypeId",
                "WorkflowId",
                "WorkflowStatus",
                "CreatedUtc")
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateMapIndexTableAsync<WorkflowBlockingActivitiesIndex>(table => table
            .Column<string>("ActivityId")
            .Column<string>("ActivityName")
            .Column<bool>("ActivityIsStart")
            .Column<string>("WorkflowTypeId")
            .Column<string>("WorkflowId")
            .Column<string>("WorkflowCorrelationId")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<WorkflowBlockingActivitiesIndex>(table => table
            .CreateIndex("IDX_WFBlockingActivities_DocumentId_ActivityId",
                "DocumentId",
                "ActivityId",
                "WorkflowTypeId",
                "WorkflowId")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<WorkflowBlockingActivitiesIndex>(table => table
            .CreateIndex("IDX_WFBlockingActivities_DocumentId_ActivityName",
                "DocumentId",
                "ActivityName",
                "WorkflowTypeId",
                "WorkflowCorrelationId")
        ).ConfigureAwait(false);

        // Shortcut other migration steps on new content definition schemas.
        return 3;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<WorkflowIndex>(table =>
        {
            table.AddColumn<string>("WorkflowStatus");
        }).ConfigureAwait(false);

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
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<WorkflowTypeStartActivitiesIndex>(table => table
            .CreateIndex("IDX_WorkflowTypeStartActivitiesIndex_DocumentId",
                "DocumentId",
                "WorkflowTypeId",
                "StartActivityId",
                "StartActivityName",
                "IsEnabled")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<WorkflowIndex>(table => table
            .CreateIndex("IDX_WorkflowIndex_DocumentId",
                "DocumentId",
                "WorkflowTypeId",
                "WorkflowId",
                "WorkflowStatus",
                "CreatedUtc")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<WorkflowBlockingActivitiesIndex>(table => table
            .CreateIndex("IDX_WFBlockingActivities_DocumentId_ActivityId",
                "DocumentId",
                "ActivityId",
                "WorkflowTypeId",
                "WorkflowId")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<WorkflowBlockingActivitiesIndex>(table => table
            .CreateIndex("IDX_WFBlockingActivities_DocumentId_ActivityName",
                "DocumentId",
                "ActivityName",
                "WorkflowTypeId",
                "WorkflowCorrelationId")
        ).ConfigureAwait(false);

        return 3;
    }
}
