using System;
using System.Linq;
using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes
{
    public class WorkflowTypeIndex : MapIndex
    {
        public long DocumentId { get; set; }
        public string WorkflowTypeId { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool HasStart { get; set; }
        public string DisplayName { get; set; }
        public string WorkflowTypeVersionId { get; set; }
        public bool Latest { get; set; }
        public DateTime CreatedUtc { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedUtc { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class WorkflowTypeStartActivitiesIndex : MapIndex
    {
        public string WorkflowTypeId { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string StartActivityId { get; set; }
        public string StartActivityName { get; set; }
    }

    public class WorkflowTypeIndexProvider : IndexProvider<WorkflowType>
    {
        public override void Describe(DescribeContext<WorkflowType> context)
        {
            context.For<WorkflowTypeIndex>()
                .Map(workflowType =>
                        new WorkflowTypeIndex
                        {
                            WorkflowTypeId = workflowType.WorkflowTypeId,
                            Name = workflowType.Name,
                            IsEnabled = workflowType.IsEnabled,
                            HasStart = workflowType.Activities.Any(x => x.IsStart),
                            DisplayName = workflowType.DisplayName,
                            WorkflowTypeVersionId = workflowType.WorkflowTypeVersionId,
                            Latest = workflowType.Latest,
                            CreatedUtc = workflowType.CreatedUtc,
                            ModifiedUtc = workflowType.ModifiedUtc,
                            ModifiedBy = workflowType.ModifiedBy,
                            CreatedBy = workflowType.CreatedBy
                        }
                );

            context.For<WorkflowTypeStartActivitiesIndex>()
                .Map(workflowType =>
                    workflowType.Activities.Where(x => x.IsStart).Select(x =>
                        new WorkflowTypeStartActivitiesIndex
                        {
                            WorkflowTypeId = workflowType.WorkflowTypeId,
                            Name = workflowType.Name,
                            IsEnabled = workflowType.IsEnabled,
                            StartActivityId = x.ActivityId,
                            StartActivityName = x.Name
                        })
                );
        }
    }
}
