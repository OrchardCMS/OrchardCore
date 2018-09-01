using System;
using System.Linq.Expressions;
using OrchardCore.Workflows.Indexes;

namespace OrchardCore.Workflows.Specifications
{
    public class SingleWorkflowSpecification : Specification<WorkflowIndex>
    {
        private readonly string _workflowId;

        public SingleWorkflowSpecification(string workflowId)
        {
            _workflowId = workflowId;
        }

        public override Expression<Func<WorkflowIndex, bool>> PredicateExpression => index => index.WorkflowId == _workflowId;
    }
}
