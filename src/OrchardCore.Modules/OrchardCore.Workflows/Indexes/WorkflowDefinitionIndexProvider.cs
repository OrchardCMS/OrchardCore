using System.Linq;
using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes
{
    public class WorkflowDefinitionByNameIndex : MapIndex
    {
        public string Name { get; set; }
    }

    public class WorkflowDefinitionByStartActivityIndex : MapIndex
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool HasStart { get; set; }
        public string StartActivityName { get; set; }
    }

    public class WorkflowDefinitionIndexProvider : IndexProvider<WorkflowDefinition>
    {
        public override void Describe(DescribeContext<WorkflowDefinition> context)
        {
            context.For<WorkflowDefinitionByNameIndex>()
                .Map(workflowDefinition => new WorkflowDefinitionByNameIndex
                {
                    Name = workflowDefinition.Name
                });

            context.For<WorkflowDefinitionByStartActivityIndex>()
                .Map(workflowDefinition =>
                {
                    var startActivity = workflowDefinition.Activities.FirstOrDefault(x => x.IsStart);
                    return new WorkflowDefinitionByStartActivityIndex
                    {
                        Name = workflowDefinition.Name,
                        IsEnabled = workflowDefinition.IsEnabled,
                        HasStart = startActivity != null,
                        StartActivityName = startActivity?.Name
                    };
                });
        }
    }
}