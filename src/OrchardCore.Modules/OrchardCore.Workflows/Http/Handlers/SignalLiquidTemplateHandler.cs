using System.Threading.Tasks;
using Fluid;
using OrchardCore.Liquid;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.WorkflowContextProviders
{
    public class SignalLiquidTemplateHandler : ILiquidTemplateEventHandler
    {
        private readonly ISecurityTokenService _signalService;

        public SignalLiquidTemplateHandler(ISecurityTokenService signalService)
        {
            _signalService = signalService;
        }

        public Task RenderingAsync(TemplateContext context)
        {
            context.AmbientValues.Add("SignalService", _signalService);
            return Task.CompletedTask;
        }
    }
}
