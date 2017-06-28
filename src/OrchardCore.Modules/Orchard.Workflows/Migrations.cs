using Orchard.Data.Migration;
using Orchard.Workflows.Indexes;

namespace Orchard.Workflows
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

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowDefinitionIndexProvider), table => table
                .Column<string>("Name")
            );

            return 1;
        }
    }
}