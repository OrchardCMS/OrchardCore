using System;
using System.Collections.Generic;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Scripting
{
    public class WorkflowMethodsProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _workflowMethod;
        private readonly GlobalMethod _workflowInstanceIdMethod;
        private readonly GlobalMethod _inputMethod;
        private readonly GlobalMethod _outputMethod;
        private readonly GlobalMethod _propertyMethod;
        private readonly GlobalMethod _resultMethod;
        private readonly GlobalMethod _correlationIdMethod;

        public WorkflowMethodsProvider(WorkflowExecutionContext workflowContext)
        {
            _workflowMethod = new GlobalMethod
            {
                Name = "workflow",
                Method = serviceProvider => (Func<object>)(() => workflowContext)
            };

            _workflowInstanceIdMethod = new GlobalMethod
            {
                Name = "workflowInstanceId",
                Method = serviceProvider => (Func<string>)(() => workflowContext.WorkflowRecord.WorkflowId)
            };

            _inputMethod = new GlobalMethod
            {
                Name = "input",
                Method = serviceProvider => (Func<string, object>)(name => workflowContext.Input[name])
            };

            _outputMethod = new GlobalMethod
            {
                Name = "output",
                Method = serviceProvider => (Action<string, object>)((name, value) => workflowContext.Output[name] = value)
            };

            _propertyMethod = new GlobalMethod
            {
                Name = "property",
                Method = serviceProvider => (Func<string, object>)((name) => workflowContext.Properties[name])
            };

            _resultMethod = new GlobalMethod
            {
                Name = "lastResult",
                Method = serviceProvider => (Func<object>)(() => workflowContext.LastResult)
            };

            _correlationIdMethod = new GlobalMethod
            {
                Name = "correlationId",
                Method = serviceProvider => (Func<string>)(() => workflowContext.WorkflowRecord.CorrelationId)
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _workflowMethod, _workflowInstanceIdMethod, _inputMethod, _outputMethod, _propertyMethod, _resultMethod, _correlationIdMethod };
        }
    }
}
