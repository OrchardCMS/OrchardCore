using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Liquid;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Liquid
{
    public class SignalUrlFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("UrlHelper", out var urlHelperObj))
            {
                throw new ArgumentException("UrlHelper missing while invoking 'signal_url'");
            }

            if (!context.AmbientValues.TryGetValue("SignalService", out var signalServiceObj))
            {
                throw new ArgumentException("SignalService missing while invoking 'signal_url'");
            }

            var correlationId = default(string);
            var correlationIdValue = arguments.At(0);
            if (correlationIdValue.IsNil())
            {
                var workflowContextValue = context.GetValue(nameof(WorkflowExecutionContext));

                if (workflowContextValue.IsNil())
                {
                    throw new ArgumentException("WorkflowContext missing and no correlation ID provided while invoking 'signal_url'");
                }

                var workflowContext = (WorkflowExecutionContext)workflowContextValue.ToObjectValue();
                correlationId = workflowContext.CorrelationId;
            }
            else
            {
                correlationId = correlationIdValue.ToStringValue();
            }

            var urlHelper = (IUrlHelper)urlHelperObj;
            var signalService = (ISignalService)signalServiceObj;
            var signalName = input.ToStringValue();

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                var workflowContextValue = context.GetValue(nameof(WorkflowExecutionContext));

                if (workflowContextValue.IsNil())
                {
                    throw new ArgumentException("WorkflowContext missing and no correlation ID provided while invoking 'signal_url'");
                }

                var workflowContext = (WorkflowExecutionContext)workflowContextValue.ToObjectValue();
                correlationId = workflowContext.CorrelationId;
            }

            var token = signalService.CreateToken(correlationId, signalName);
            var urlValue = new StringValue(urlHelper.Action("Trigger", "Signal", new { area = "OrchardCore.Workflows", token }));
            return Task.FromResult<FluidValue>(urlValue);
        }
    }
}
