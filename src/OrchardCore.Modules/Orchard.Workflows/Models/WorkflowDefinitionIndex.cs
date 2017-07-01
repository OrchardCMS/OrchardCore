using YesSql.Indexes;

namespace Orchard.Workflows.Models
{
    public class WorkflowDefinitionIndex : MapIndex
    {
        public string Name { get; set; }
    }

    public class WorkflowDefinitionIndexProvider : IndexProvider<WorkflowDefinition>
    {
        public override void Describe(DescribeContext<WorkflowDefinition> context)
        {
            context.For<WorkflowDefinitionIndex>()
                .Map(wf =>
                {
                    return new WorkflowDefinitionIndex
                    {
                         Name = wf.Name
                    };
                });
        }
    }
}