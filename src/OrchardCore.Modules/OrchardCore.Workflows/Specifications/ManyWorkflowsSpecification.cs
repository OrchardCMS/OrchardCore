using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OrchardCore.Workflows.Indexes;
using YesSql.Services;

namespace OrchardCore.Workflows.Specifications
{
    public class ManyWorkflowsSpecification : Specification<WorkflowIndex>
    {
        private readonly IEnumerable<string> _workflowIds;

        public ManyWorkflowsSpecification(IEnumerable<string> workflowIds)
        {
            _workflowIds = workflowIds.ToList();
        }

        public override Expression<Func<WorkflowIndex, bool>> PredicateExpression => index => index.WorkflowId.IsIn(_workflowIds);
    }
}
