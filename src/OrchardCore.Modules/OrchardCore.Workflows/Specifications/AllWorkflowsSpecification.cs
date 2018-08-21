using System;
using System.Linq.Expressions;
using OrchardCore.Workflows.Indexes;

namespace OrchardCore.Workflows.Specifications
{
    public class AllWorkflowsSpecification : Specification<WorkflowIndex>
    {
        public override Expression<Func<WorkflowIndex, object>> OrderByDescendingExpression => x => x.CreatedUtc;
    }
}
