using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Scripting
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
                    var payload = !String.IsNullOrWhiteSpace(workflowContext.CorrelationId) ? SignalPayload.ForCorrelation(signal, workflowContext.CorrelationId) : SignalPayload.ForWorkflow(signal, workflowContext.WorkflowId);
                    var token = signalService.CreateToken(payload, TimeSpan.FromDays(7));
                    var urlHelper = serviceProvider.GetRequiredService<IUrlHelper>();
                    return urlHelper.Action("Trigger", "HttpWorkflow", new { area = "OrchardCore.Workflows", token });
                })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _signalUrlMethod };
        }
    }
}
