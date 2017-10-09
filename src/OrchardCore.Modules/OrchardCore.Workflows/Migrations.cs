using Orchard.Data.Migration;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows
{
    public class Migrations : DataMigration
    {
        public int Create() {
            SchemaBuilder.CreateMapIndexTable(nameof(ActivityIndex), table => table
                .Column<string>("Name")
                .Column<bool>("DefinitionEnabled")
                .Column<bool>("Start")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(AwaitingActivityIndex), table => table
                .Column<string>("ActivityName")
                .Column<bool>("ActivityStart")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowDefinitionIndex), table => table
                .Column<string>("Name")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowWorkflowDefinitionIndex), table => table
                .Column<int>("DefinitionId")
            );

            return 1;
        }
    }
}