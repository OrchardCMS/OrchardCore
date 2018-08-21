using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OrchardCore.Workflows.Indexes;
using YesSql.Services;

namespace OrchardCore.Workflows.Specifications
{
    public class ManyWorkflowsByTypesSpecification : Specification<WorkflowIndex>
    {
        private readonly IEnumerable<string> _workflowTypeIds;

        public ManyWorkflowsByTypesSpecification(IEnumerable<string> workflowTypeIds)
        {
            _workflowTypeIds = workflowTypeIds.ToList();
        }

        public override Expression<Func<WorkflowIndex, bool>> PredicateExpression => index => index.WorkflowTypeId.IsIn(_workflowTypeIds);
    }
}
