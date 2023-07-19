using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Http.Controllers;
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
                    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                    var linkGenerator = serviceProvider.GetRequiredService<LinkGenerator>();

                    return linkGenerator.GetPathByAction(httpContextAccessor.HttpContext, "Trigger", "HttpWorkflow", new { area = "OrchardCore.Workflows", token });
                }),
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
                Method = serviceProvider => (Func<string, string, int, string>)((workflowTypeId, activityId, days) =>
                {
                    var securityTokenService = serviceProvider.GetRequiredService<ISecurityTokenService>();

                    var payload = new WorkflowPayload(workflowTypeId, activityId);

                    if (days == 0)
                    {
                        days = HttpWorkflowController.NoExpiryTokenLifespan;
                    }

                    return securityTokenService.CreateToken(payload, TimeSpan.FromDays(days));
                })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _createWorkflowToken };
        }
    }
}
