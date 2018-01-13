using System.Threading.Tasks;
using Fluid;
using OrchardCore.Liquid;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.WorkflowContextProviders
{
    public class SignalLiquidTemplateHandler : ILiquidTemplateEventHandler
    {
        private readonly ISignalService _signalService;

        public SignalLiquidTemplateHandler(ISignalService signalService)
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
