using System.Threading.Tasks;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Scripting;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.WorkflowContextProviders
{
    public class SignalWorkflowContextHandler : WorkflowContextHandlerBase
    {
        private readonly ISignalService _signalService;

        public SignalWorkflowContextHandler(ISignalService signalService)
        {
            _signalService = signalService;
        }

        public override Task EvaluatingScriptAsync(WorkflowContextScriptEvalContext context)
        {
            context.ScopedMethodProviders.Add(new SignalMethodProvider(context.WorkflowContext, _signalService));
            return Task.CompletedTask;
        }
    }
}
