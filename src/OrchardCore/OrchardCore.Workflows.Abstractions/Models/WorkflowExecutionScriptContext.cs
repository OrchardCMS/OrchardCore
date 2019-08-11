using System.Collections.Generic;
using OrchardCore.Scripting;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowExecutionScriptContext : WorkflowExecutionHandlerContextBase
    {
        public WorkflowExecutionScriptContext(WorkflowExecutionContext workflowContext) : base(workflowContext)
        {
        }

        public IList<IGlobalMethodProvider> ScopedMethodProviders { get; } = new List<IGlobalMethodProvider>();
    }
}
