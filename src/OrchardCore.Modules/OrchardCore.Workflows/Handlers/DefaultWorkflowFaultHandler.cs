using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Workflows.Events;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Handlers
{
    public class DefaultWorkflowFaultHandler : IWorkflowFaultHandler
    {
        public async Task OnWorkflowFaultAsync(
            IWorkflowManager workflowManager,
            WorkflowExecutionContext workflowContext,
            ActivityContext activityContext,
            Exception exception)
        {
            var name = nameof(WorkflowFaultEvent);
            var faultContext = new WorkflowFaultModel()
            {
                WorkflowId = workflowContext.Workflow.WorkflowId,
                WorkflowName = workflowContext.WorkflowType.Name,
                ExecutedActivityCount = workflowContext.ExecutedActivities.Count,
                FaultMessage = workflowContext.Workflow.FaultMessage,
                ActivityId = activityContext.ActivityRecord.ActivityId,
                ActivityTypeName = activityContext.Activity.Name,
                ActivityDisplayName = activityContext.ActivityRecord.Properties["ActivityMetadata"]?["Title"].ToString(),
                ExceptionDetails = exception?.ToString(),
                ErrorMessage = exception.Message,
            };

            var input = new Dictionary<string, object>
            {
                { WorkflowFaultModel.WorkflowFaultInputKey, faultContext },
            };

            await workflowManager.TriggerEventAsync(name, input);
        }
    }
}
