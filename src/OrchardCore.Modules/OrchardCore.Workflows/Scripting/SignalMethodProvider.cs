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

        public SignalMethodProvider(WorkflowContext workflowContext, ISignalService signalService)
        {
            _signalUrlMethod = new GlobalMethod
            {
                Name = "signalUrl",
                Method = serviceProvider => (Func<string, string>)((signal) =>
                {
                    var token = signalService.CreateToken(workflowContext.CorrelationId, signal);
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
