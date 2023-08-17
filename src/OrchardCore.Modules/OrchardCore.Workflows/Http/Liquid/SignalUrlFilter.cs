using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using OrchardCore.Liquid;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Liquid
{
    public class SignalUrlFilter : ILiquidFilter
    {
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly ISecurityTokenService _securityTokenService;

        public SignalUrlFilter(IUrlHelperFactory urlHelperFactory, ISecurityTokenService securityTokenService)
        {
            _urlHelperFactory = urlHelperFactory;
            _securityTokenService = securityTokenService;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(ctx.ViewContext);

            var workflowContextValue = ctx.GetValue("Workflow");

            if (workflowContextValue.IsNil())
            {
                throw new ArgumentException("WorkflowExecutionContext missing while invoking 'signal_url'");
            }

            var workflowContext = (WorkflowExecutionContext)workflowContextValue.ToObjectValue();
            var signalName = input.ToStringValue();
            var payload = String.IsNullOrWhiteSpace(workflowContext.CorrelationId)
                ? SignalPayload.ForWorkflow(signalName, workflowContext.WorkflowId)
                : SignalPayload.ForCorrelation(signalName, workflowContext.CorrelationId);

            var token = _securityTokenService.CreateToken(payload, TimeSpan.FromDays(7));
            var urlValue = new StringValue(urlHelper.Action("Trigger", "HttpWorkflow", new { area = "OrchardCore.Workflows", token }));

            return new ValueTask<FluidValue>(urlValue);
        }
    }
}
