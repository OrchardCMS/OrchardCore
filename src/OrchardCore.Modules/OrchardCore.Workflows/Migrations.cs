using System;
using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Workflows
{
    public class Migrations : DataMigration
    {
        private readonly IWorkflowStore _workflowStore;
        private readonly IWorkflowTypeStore _workflowTypeStore;
        private readonly ISession _session;
        private readonly IClock _clock;

        public Migrations(IWorkflowStore workflowStore, IWorkflowTypeStore workflowTypeStore, ISession session, IClock clock)
        {
            _workflowStore = workflowStore;
            _workflowTypeStore = workflowTypeStore;
            _session = session;
            _clock = clock;
        }

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
            return 3;
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
        public async Task<int> UpdateFrom3Async()
        {
            await SchemaBuilder.AlterIndexTableAsync<WorkflowTypeIndex>(table =>
            {
                table.AddColumn<string>("DisplayName", c => c.WithLength(255));
                table.AddColumn<string>("WorkflowTypeVersionId", c => c.WithLength(26));
                table.AddColumn<bool>("Latest");
                table.AddColumn<bool>("UpdatedBy");
                table.AddColumn<DateTime>("CreatedUtc");
                table.AddColumn<DateTime>("ModifiedUtc");
            });

            await SchemaBuilder.AlterIndexTableAsync<WorkflowTypeIndex>(table => table
                .CreateIndex("IDX_WorkflowTypeIndex_DocumentId",
                        "DocumentId",
                        "WorkflowTypeId",
                        "Name",
                        "HasStart",
                        "IsEnabled",
                        "WorkflowTypeVersionId",
                        "DisplayName",
                        "Latest",
                        "CreatedUtc",
                        "ModifiedUtc",
                        "UpdatedBy")
            );

            //await SchemaBuilder.AlterIndexTableAsync<WorkflowTypeStartActivitiesIndex>(table =>
            //{
            //    table.AddColumn<string>("WorkflowTypeVersionId", c => c.WithLength(26));
            //    table.CreateIndex("IDX_WorkflowTypeStartActivitiesIndex_DocumentId",
            //        "DocumentId",
            //        "WorkflowTypeId",
            //        "WorkflowTypeVersionId",
            //        "StartActivityId",
            //        "StartActivityName",
            //        "IsEnabled");
            //});

            await SchemaBuilder.AlterIndexTableAsync<WorkflowIndex>(table =>
            {
                table.AddColumn<string>("WorkflowTypeVersionId", c => c.WithLength(26));
                table.CreateIndex("IDX_WorkflowIndex_DocumentId",
                    "DocumentId",
                    "WorkflowTypeId",
                    "WorkflowTypeVersionId",
                    "WorkflowId",
                    "WorkflowStatus",
                    "CreatedUtc");
            });


            await SchemaBuilder.AlterIndexTableAsync<WorkflowBlockingActivitiesIndex>(table =>
            {
                table.AddColumn<string>("WorkflowTypeVersionId", c => c.WithLength(26));
            });

            await SchemaBuilder.AlterIndexTableAsync<WorkflowBlockingActivitiesIndex>(table => table
                .CreateIndex("IDX_WFBlockingActivities_DocumentId_ActivityId",
                                    "DocumentId",
                                    "ActivityId",
                                    "WorkflowTypeId",
                                    "WorkflowTypeVersionId",
                                    "WorkflowId"));

            await SchemaBuilder.AlterIndexTableAsync<WorkflowBlockingActivitiesIndex>(table => table
                .CreateIndex("IDX_WFBlockingActivities_DocumentId_ActivityName",
                    "DocumentId",
                    "ActivityName",
                    "WorkflowTypeId",
                    "WorkflowTypeVersionId",
                    "WorkflowCorrelationId"));

            var existsedTypes = await _session.Query<WorkflowType, WorkflowTypeIndex>().ListAsync();

            foreach (var workflowType in existsedTypes)
            {
                workflowType.DisplayName = workflowType.Name;
                workflowType.Latest = true;
                workflowType.CreatedUtc = _clock.UtcNow;
                workflowType.ModifiedUtc = _clock.UtcNow;

                await _workflowTypeStore.SaveAsync(workflowType);
            }

            return 4;
        }



    }
}
