using System.Collections.Generic;
using OrchardCore.Scripting;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowContextScriptEvalContext : WorkflowContextHandlerContextBase
    {
        public WorkflowContextScriptEvalContext(WorkflowContext workflowContext) : base(workflowContext)
        {
        }

        public IList<IGlobalMethodProvider> ScopedMethodProviders { get; } = new List<IGlobalMethodProvider>();
    }
}
