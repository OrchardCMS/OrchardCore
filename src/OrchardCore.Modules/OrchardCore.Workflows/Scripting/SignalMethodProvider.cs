using System;
using System.Collections.Generic;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Scripting
{
    public class SignalMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _nonceMethod;

        public SignalMethodProvider(WorkflowContext workflowContext, ISignalService signalService)
        {
            _nonceMethod = new GlobalMethod
            {
                Name = "createSignalToken",
                Method = serviceProvider => (Func<string, string>)((signal) => signalService.CreateToken(workflowContext.CorrelationId, signal))
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _nonceMethod };
        }
    }
}
