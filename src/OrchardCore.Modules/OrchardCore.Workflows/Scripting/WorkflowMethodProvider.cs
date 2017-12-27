using System;
using System.Collections.Generic;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Scripting
{
    public class WorkflowMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _workflowMethod;
        private readonly GlobalMethod _inputMethod;
        private readonly GlobalMethod _resultMethod;

        public WorkflowMethodProvider(WorkflowContext workflowContext)
        {
            _workflowMethod = new GlobalMethod
            {
                Name = "workflow",
                Method = serviceProvider => (Func<object>)(() => workflowContext)
            };

            _inputMethod = new GlobalMethod
            {
                Name = "input",
                Method = serviceProvider => (Func<string, object>)(name => workflowContext.Input[name])
            };

            _resultMethod = new GlobalMethod
            {
                Name = "result",
                Method = serviceProvider => (Func<object>)(() => workflowContext.Stack.Pop())
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _workflowMethod, _inputMethod, _resultMethod };
        }
    }
}
