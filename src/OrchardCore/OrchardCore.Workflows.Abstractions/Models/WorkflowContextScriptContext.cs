using System.Collections.Generic;
using OrchardCore.Scripting;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowContextScriptContext : WorkflowContextHandlerContextBase
    {
        public WorkflowContextScriptContext(WorkflowContext workflowContext) : base(workflowContext)
        {
        }

        public IList<IGlobalMethodProvider> ScopedMethodProviders { get; } = new List<IGlobalMethodProvider>();
    }
}
