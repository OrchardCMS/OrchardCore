using System.Threading.Tasks;
using OrchardCore.Workflows.Http.Scripting;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.WorkflowContextProviders
{
    public class SignalWorkflowExecutionContextHandler : WorkflowExecutionContextHandlerBase
    {
        private readonly ISecurityTokenService _securityTokenService;

        public SignalWorkflowExecutionContextHandler(ISecurityTokenService securityTokenService)
        {
            _securityTokenService = securityTokenService;
        }

        public override Task EvaluatingScriptAsync(WorkflowExecutionScriptContext context)
        {
            context.ScopedMethodProviders.Add(new SignalMethodProvider(context.WorkflowContext, _securityTokenService));
            return Task.CompletedTask;
        }
    }
}
