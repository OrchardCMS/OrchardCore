using OrchardCore.Data.Migration;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowDefinitionIndex), table => table
                .Column<string>("Name")
                .Column<bool>("IsEnabled")
                .Column<bool>("HasStart")
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
                .Column<string>("Mode")
                .Column<string>("HttpMethod")
                .Column<string>("RequestPath")
            );

            return 1;
        }
    }
}