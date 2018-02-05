using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Scripting
{
    public class SignalMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _signalUrlMethod;

        public SignalMethodProvider(WorkflowExecutionContext workflowContext, ISecurityTokenService signalService)
        {
            _signalUrlMethod = new GlobalMethod
            {
                Name = "signalUrl",
                Method = serviceProvider => (Func<string, string>)((signal) =>
                {
                    var payload = !string.IsNullOrWhiteSpace(workflowContext.CorrelationId) ? SignalPayload.ForCorrelation(signal, workflowContext.CorrelationId) : SignalPayload.ForWorkflowInstance(signal, workflowContext.WorkflowInstanceId);
                    var token = signalService.CreateToken(payload);
                    var urlHelper = serviceProvider.GetRequiredService<IUrlHelper>();
                    return urlHelper.Action("Trigger", "Signal", new { area = "OrchardCore.Workflows", token });
                })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _signalUrlMethod };
        }
    }
}
