using System;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowFaultHandler
    {
        Task OnWorkflowFaultAsync(
            IWorkflowManager workflowManager,
            WorkflowExecutionContext workflowContext,
            ActivityContext activityContext,
            Exception exception);
    }
}
