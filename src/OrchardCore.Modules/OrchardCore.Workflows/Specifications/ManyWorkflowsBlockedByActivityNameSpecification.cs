using System;
using System.Linq.Expressions;
using OrchardCore.Workflows.Indexes;

namespace OrchardCore.Workflows.Specifications
{
    public class ManyWorkflowsBlockedByActivityNameSpecification : Specification<WorkflowBlockingActivitiesIndex>
    {
        private readonly string _activityName;
        private readonly string _correlationId;

        public ManyWorkflowsBlockedByActivityNameSpecification(string activityName, string correlationId = null)
        {
            _activityName = activityName;
            _correlationId = correlationId;
        }

        public override Expression<Func<WorkflowBlockingActivitiesIndex, bool>> PredicateExpression => index =>
            index.ActivityName == _activityName &&
            index.WorkflowCorrelationId == (_correlationId ?? "");
    }
}
