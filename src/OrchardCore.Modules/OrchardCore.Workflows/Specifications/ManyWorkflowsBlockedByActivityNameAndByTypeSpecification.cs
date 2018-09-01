using System;
using System.Linq.Expressions;
using OrchardCore.Workflows.Indexes;

namespace OrchardCore.Workflows.Specifications
{
    public class ManyWorkflowsBlockedByActivityNameAndByTypeSpecification : Specification<WorkflowBlockingActivitiesIndex>
    {
        private readonly string _workflowTypeId;
        private readonly string _activityName;
        private readonly string _correlationId;

        public ManyWorkflowsBlockedByActivityNameAndByTypeSpecification(string workflowTypeId, string activityName, string correlationId = null)
        {
            _workflowTypeId = workflowTypeId;
            _activityName = activityName;
            _correlationId = correlationId;
        }

        public override Expression<Func<WorkflowBlockingActivitiesIndex, bool>> PredicateExpression => index =>
            index.WorkflowTypeId == _workflowTypeId &&
            index.ActivityName == _activityName &&
            index.WorkflowCorrelationId == (_correlationId ?? "");
    }
}
