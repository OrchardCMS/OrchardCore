using System;
using System.Linq.Expressions;
using OrchardCore.Workflows.Indexes;

namespace OrchardCore.Workflows.Specifications
{
    public class ManyWorkflowsByTypeSpecification : Specification<WorkflowIndex>
    {
        private readonly string _workflowTypeId;

        public ManyWorkflowsByTypeSpecification(string workflowTypeId)
        {
            _workflowTypeId = workflowTypeId;
        }

        public override Expression<Func<WorkflowIndex, bool>> PredicateExpression => index => index.WorkflowTypeId == _workflowTypeId;
    }
}
