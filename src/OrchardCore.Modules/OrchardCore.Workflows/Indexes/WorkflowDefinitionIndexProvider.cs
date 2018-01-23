using System.Linq;
using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes
{
    public class WorkflowDefinitionIndex : MapIndex
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool HasStart { get; set; }
    }

    public class WorkflowDefinitionStartActivitiesIndex : MapIndex
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public int StartActivityId { get; set; }
        public string StartActivityName { get; set; }
    }

    public class WorkflowDefinitionIndexProvider : IndexProvider<WorkflowDefinitionRecord>
    {
        public override void Describe(DescribeContext<WorkflowDefinitionRecord> context)
        {
            context.For<WorkflowDefinitionIndex>()
                .Map(workflowDefinition =>
                        new WorkflowDefinitionIndex
                        {
                            Uid = workflowDefinition.Uid,
                            Name = workflowDefinition.Name,
                            IsEnabled = workflowDefinition.IsEnabled,
                            HasStart = workflowDefinition.Activities.Any(x => x.IsStart)
                        }
                );

            context.For<WorkflowDefinitionStartActivitiesIndex>()
                .Map(workflowDefinition =>
                    workflowDefinition.Activities.Where(x => x.IsStart).Select(x =>
                        new WorkflowDefinitionStartActivitiesIndex
                        {
                            Uid = workflowDefinition.Uid,
                            Name = workflowDefinition.Name,
                            IsEnabled = workflowDefinition.IsEnabled,
                            StartActivityId = x.Id,
                            StartActivityName = x.Name
                        })
                );
        }
    }
}