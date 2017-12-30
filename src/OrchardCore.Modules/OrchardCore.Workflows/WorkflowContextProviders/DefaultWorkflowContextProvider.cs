using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Scripting;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.WorkflowContextProviders
{
    public class DefaultWorkflowContextProvider : IWorkflowContextProvider
    {
        public void Configure(WorkflowContext workflowContext)
        {
            workflowContext.ScriptingManager.GlobalMethodProviders.Add(new WorkflowMethodProvider(workflowContext));
        }
    }
}
