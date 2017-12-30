using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    /// <summary>
    /// Allows modules an opportunity to provide additional information and services to the specified <see cref="WorkflowContext"/>.
    /// For example, to add global script method providers that is scoped to the workflow context.
    /// </summary>
    public interface IWorkflowContextProvider
    {
        void Configure(WorkflowContext workflowContext);
    }
}
