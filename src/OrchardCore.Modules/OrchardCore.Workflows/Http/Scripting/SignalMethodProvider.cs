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

        public SignalMethodProvider(WorkflowExecutionContext workflowContext, ISecurityTokenService securityTokenService)
        {
            _signalUrlMethod = new GlobalMethod
            {
                Name = "signalUrl",
                Method = serviceProvider => (Func<string, string>)((signal) =>
                {
                    var payload = !String.IsNullOrWhiteSpace(workflowContext.CorrelationId) ? SignalPayload.ForCorrelation(signal, workflowContext.CorrelationId) : SignalPayload.ForWorkflow(signal, workflowContext.WorkflowId);
                    var token = securityTokenService.CreateToken(payload, TimeSpan.FromDays(7));
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

    public class TokenMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _createWorkflowToken;

        public TokenMethodProvider()
        {
            _createWorkflowToken = new GlobalMethod
            {
                Name = "createWorkflowToken",
                Method = serviceProvider => (Func<string, string, string, string>)((workflowTypeId, activityId, lifetime) =>
                {
                    var securityTokenService = serviceProvider.GetRequiredService<ISecurityTokenService>();

                    var payload = new WorkflowPayload(workflowTypeId, activityId);
                    if (!TimeSpan.TryParse(lifetime, out var timespan))
                    {
                        timespan = TimeSpan.FromDays(7);
                    }

                    var token = securityTokenService.CreateToken(payload, timespan);
                    return token;
                })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _createWorkflowToken };
        }
    }
}
