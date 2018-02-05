using System.Threading.Tasks;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Scripting;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.WorkflowContextProviders
{
    public class SignalWorkflowExecutionContextHandler : WorkflowExecutionContextHandlerBase
    {
        private readonly ISecurityTokenService _signalService;

        public SignalWorkflowExecutionContextHandler(ISecurityTokenService signalService)
        {
            _signalService = signalService;
        }

        public override Task EvaluatingScriptAsync(WorkflowExecutionScriptContext context)
        {
            context.ScopedMethodProviders.Add(new SignalMethodProvider(context.WorkflowContext, _signalService));
            return Task.CompletedTask;
        }
    }
}
