using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Liquid
{
    public static class SignalUrlFilter
    {
        public static ValueTask<FluidValue> SignalUrl(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;
            var urlHelperFactory = context.Services.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory.GetUrlHelper(context.ViewContext);

            var signalService = context.Services.GetRequiredService<ISecurityTokenService>();

            var workflowContextValue = context.GetValue("Workflow");

            if (workflowContextValue.IsNil())
            {
                throw new ArgumentException("WorkflowExecutionContext missing while invoking 'signal_url'");
            }

            var workflowContext = (WorkflowExecutionContext)workflowContextValue.ToObjectValue();
            var signalName = input.ToStringValue();
            var payload = String.IsNullOrWhiteSpace(workflowContext.CorrelationId)
                ? SignalPayload.ForWorkflow(signalName, workflowContext.WorkflowId)
                : SignalPayload.ForCorrelation(signalName, workflowContext.CorrelationId);

            var token = signalService.CreateToken(payload, TimeSpan.FromDays(7));
            var urlValue = new StringValue(urlHelper.Action("Trigger", "HttpWorkflow", new { area = "OrchardCore.Workflows", token }));
            return new ValueTask<FluidValue>(urlValue);
        }
    }
}
