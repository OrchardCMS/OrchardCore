using System.Linq;
using YesSql.Indexes;

namespace Orchard.Workflows.Models
{
    public class WorkflowIndex : MapIndex
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public bool HasStart { get; set; }

    }

    public class WorkflowIndexProvider : IndexProvider<Workflow>
    {
        public override void Describe(DescribeContext<Workflow> context)
        {
            context.For<WorkflowIndex>()
                .Map(workflowDefinition =>
                {
                    return new WorkflowIndex
                    {
                        Name = workflowDefinition.Name,
                        Enabled = workflowDefinition.Enabled,
                        HasStart = workflowDefinition.Activities.Any(x => x.Start)
                    };
                });
        }
    }
}