using System.Collections.Generic;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Scripting;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowContextImpl : WorkflowContext
    {
        private readonly IScriptingManager _scriptingManager;

        public WorkflowContextImpl
        (
            WorkflowDefinitionRecord workflowDefinitionRecord,
            WorkflowInstanceRecord workflowInstanceRecord,
            IEnumerable<ActivityContext> activities,
            IScriptingManager scriptingManager
        ) : base(workflowDefinitionRecord, workflowInstanceRecord, activities)
        {

            _scriptingManager = scriptingManager;
            _scriptingManager.GlobalMethodProviders.Add(new WorkflowMethodProvider(this));
        }


        public override T Evaluate<T>(WorkflowExpression<T> expression)
        {
            return (T)_scriptingManager.Evaluate(expression.Expression);
        }
    }
}