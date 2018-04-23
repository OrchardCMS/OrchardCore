using System.Linq;
using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes
{
    public class WorkflowDefinitionIndex : MapIndex
    {
        public string WorkflowDefinitionId { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool HasStart { get; set; }
    }

    public class WorkflowDefinitionStartActivitiesIndex : MapIndex
    {
        public string WorkflowDefinitionId { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string StartActivityId { get; set; }
        public string StartActivityName { get; set; }
    }

    public class WorkflowDefinitionIndexProvider : IndexProvider<WorkflowType>
    {
        public override void Describe(DescribeContext<WorkflowType> context)
        {
            context.For<WorkflowDefinitionIndex>()
                .Map(workflowDefinitionRecord =>
                        new WorkflowDefinitionIndex
                        {
                            WorkflowDefinitionId = workflowDefinitionRecord.WorkflowTypeId,
                            Name = workflowDefinitionRecord.Name,
                            IsEnabled = workflowDefinitionRecord.IsEnabled,
                            HasStart = workflowDefinitionRecord.Activities.Any(x => x.IsStart)
                        }
                );

            context.For<WorkflowDefinitionStartActivitiesIndex>()
                .Map(workflowDefinitionRecord =>
                    workflowDefinitionRecord.Activities.Where(x => x.IsStart).Select(x =>
                        new WorkflowDefinitionStartActivitiesIndex
                        {
                            WorkflowDefinitionId = workflowDefinitionRecord.WorkflowTypeId,
                            Name = workflowDefinitionRecord.Name,
                            IsEnabled = workflowDefinitionRecord.IsEnabled,
                            StartActivityId = x.ActivityId,
                            StartActivityName = x.Name
                        })
                );
        }
    }
}