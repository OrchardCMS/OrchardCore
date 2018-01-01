using System.Linq;
using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes
{
    public class WorkflowDefinitionIndex : MapIndex
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool HasStart { get; set; }
        public string StartActivityName { get; set; }
    }

    public class WorkflowDefinitionIndexProvider : IndexProvider<WorkflowDefinitionRecord>
    {
        public override void Describe(DescribeContext<WorkflowDefinitionRecord> context)
        {
            context.For<WorkflowDefinitionIndex>()
                .Map(workflowDefinition =>
                    workflowDefinition.Activities.Select(x =>
                        new WorkflowDefinitionIndex
                        {
                            Name = workflowDefinition.Name,
                            IsEnabled = workflowDefinition.IsEnabled,
                            HasStart = x.IsStart,
                            StartActivityName = x.Name
                        })
                );
        }
    }
}