using OrchardCore.Data.Migration;
using OrchardCore.Workflows.Indexes;

namespace OrchardCore.Workflows
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowDefinitionByStartActivityIndex), table => table
                .Column<string>("Name")
                .Column<bool>("IsEnabled")
                .Column<bool>("HasStart")
                .Column<string>("StartActivityName")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowInstanceByAwaitingActivitiesIndex), table => table
                .Column<int>("ActivityId")
                .Column<string>("ActivityName")
                .Column<bool>("ActivityIsStart")
                .Column<int>("WorkflowInstanceId")
            );

            return 1;
        }
    }
}