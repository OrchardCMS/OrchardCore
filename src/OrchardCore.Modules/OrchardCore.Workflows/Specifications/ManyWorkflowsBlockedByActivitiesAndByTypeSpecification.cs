using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OrchardCore.Workflows.Indexes;
using YesSql.Services;

namespace OrchardCore.Workflows.Specifications
{
    public class ManyWorkflowsBlockedByActivitiesAndByTypeSpecification : Specification<WorkflowBlockingActivitiesIndex>
    {
        private readonly string _workflowTypeId;
        private readonly IEnumerable<string> _blockingActivityIds;

        public ManyWorkflowsBlockedByActivitiesAndByTypeSpecification(string workflowTypeId, IEnumerable<string> blockingActivityIds)
        {
            _workflowTypeId = workflowTypeId;
            _blockingActivityIds = blockingActivityIds.ToList();
        }

        public override Expression<Func<WorkflowBlockingActivitiesIndex, bool>> PredicateExpression => index =>
            index.WorkflowTypeId == _workflowTypeId &&
            index.ActivityId.IsIn(_blockingActivityIds);
    }
}
