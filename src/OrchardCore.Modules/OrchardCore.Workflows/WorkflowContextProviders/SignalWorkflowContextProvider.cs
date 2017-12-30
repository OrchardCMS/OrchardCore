using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Scripting;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.WorkflowContextProviders
{
    public class SignalWorkflowContextProvider : IWorkflowContextProvider
    {
        private readonly ISignalService _signalService;

        public SignalWorkflowContextProvider(ISignalService signalService)
        {
            _signalService = signalService;
        }

        public void Configure(WorkflowContext workflowContext)
        {
            workflowContext.ScriptingManager.GlobalMethodProviders.Add(new SignalMethodProvider(workflowContext, _signalService));
        }
    }
}
