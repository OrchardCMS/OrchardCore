using System;
using OrchardCore.Data.Migration;
using OrchardCore.Workflows.Indexes;

namespace OrchardCore.Workflows
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowTypeIndex), table => table
                .Column<string>("WorkflowTypeId")
                .Column<string>("Name")
                .Column<bool>("IsEnabled")
                .Column<bool>("HasStart")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowTypeStartActivitiesIndex), table => table
                .Column<string>("WorkflowTypeId")
                .Column<string>("Name")
                .Column<bool>("IsEnabled")
                .Column<string>("StartActivityId")
                .Column<string>("StartActivityName")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowIndex), table => table
                .Column<string>("WorkflowTypeId")
                .Column<string>("WorkflowId")
                .Column<string>("WorkflowStatus")
                .Column<DateTime>("CreatedUtc")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(WorkflowBlockingActivitiesIndex), table => table
                .Column<string>("ActivityId")
                .Column<string>("ActivityName")
                .Column<bool>("ActivityIsStart")
                .Column<string>("WorkflowTypeId")
                .Column<string>("WorkflowId")
                .Column<string>("WorkflowCorrelationId")
            );

            return 2;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterTable(nameof(WorkflowIndex), table =>
            {
                table.AddColumn<string>("WorkflowStatus");
            });

            return 2;
        }
    }
}
