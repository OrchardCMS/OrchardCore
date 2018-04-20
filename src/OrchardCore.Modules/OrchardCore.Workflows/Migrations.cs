using System;
using OrchardCore.Data.Migration;
using OrchardCore.Workflows.Indexes;

namespace OrchardCore.Workflows
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowDefinitionIndex), table => table
                .Column<string>("WorkflowDefinitionId")
                .Column<string>("Name")
                .Column<bool>("IsEnabled")
                .Column<bool>("HasStart")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowDefinitionStartActivitiesIndex), table => table
                .Column<string>("WorkflowDefinitionId")
                .Column<string>("Name")
                .Column<bool>("IsEnabled")
                .Column<string>("StartActivityId")
                .Column<string>("StartActivityName")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowInstanceIndex), table => table
                .Column<string>("WorkflowDefinitionId")
                .Column<string>("WorkflowInstanceId")
                .Column<DateTime>("CreatedUtc")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowInstanceBlockingActivitiesIndex), table => table
                .Column<string>("ActivityId")
                .Column<string>("ActivityName")
                .Column<bool>("ActivityIsStart")
                .Column<string>("WorkflowDefinitionId")
                .Column<string>("WorkflowInstanceId")
                .Column<string>("WorkflowInstanceCorrelationId")
            );

            return 1;
        }
    }
}