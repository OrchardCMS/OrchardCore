using System.Threading.Tasks;
using OrchardCore.Workflows.Http.Scripting;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.WorkflowContextProviders
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
