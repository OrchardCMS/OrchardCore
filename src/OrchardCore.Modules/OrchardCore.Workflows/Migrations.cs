using OrchardCore.Data.Migration;
using OrchardCore.Workflows.Indexes;

namespace OrchardCore.Workflows
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowDefinitionsIndex), table => table
                .Column<string>("Name")
                .Column<bool>("IsEnabled")
                .Column<bool>("HasStart")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowDefinitionStartActivitiesIndex), table => table
                .Column<string>("Name")
                .Column<bool>("IsEnabled")
                .Column<int>("StartActivityId")
                .Column<string>("StartActivityName")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowInstanceByAwaitingActivitiesIndex), table => table
                .Column<string>("ActivityId")
                .Column<string>("ActivityName")
                .Column<bool>("ActivityIsStart")
                .Column<int>("WorkflowInstanceId")
                .Column<string>("WorkflowInstanceCorrelationId")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowDefinitionByHttpRequestIndex), table => table
                .Column<int>("WorkflowDefinitionId")
                .Column<int>("ActivityId")
                .Column<string>("HttpMethod")
                .Column<string>("RequestPath")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowDefinitionByHttpRequestFilterIndex), table => table
                .Column<int>("WorkflowDefinitionId")
                .Column<int>("ActivityId")
                .Column<string>("HttpMethod")
                .Column<string>("ControllerName")
                .Column<string>("ActionName")
                .Column<string>("AreaName")
            );

            return 1;
        }
    }
}