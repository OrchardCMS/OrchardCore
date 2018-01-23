using OrchardCore.Data.Migration;
using OrchardCore.Workflows.Indexes;

namespace OrchardCore.Workflows
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowDefinitionIndex), table => table
                .Column<string>("Uid")
                .Column<string>("Name")
                .Column<bool>("IsEnabled")
                .Column<bool>("HasStart")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowDefinitionStartActivitiesIndex), table => table
                .Column<string>("Uid")
                .Column<string>("Name")
                .Column<bool>("IsEnabled")
                .Column<int>("StartActivityId")
                .Column<string>("StartActivityName")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowInstanceIndex), table => table
                .Column<string>("WorkflowDefinitionUid")
                .Column<string>("WorkflowInstanceUid")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowInstanceByAwaitingActivitiesIndex), table => table
                .Column<int>("ActivityId")
                .Column<string>("ActivityName")
                .Column<bool>("ActivityIsStart")
                .Column<string>("WorkflowInstanceUid")
                .Column<string>("WorkflowInstanceCorrelationId")
            );

            return 1;
        }
    }
}