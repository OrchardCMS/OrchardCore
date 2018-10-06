using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities
{
    public abstract class TenantEvent : TenantActivity, IEvent
    {
        protected TenantEvent(IShellSettingsManager shellSettingsManager, IShellHost shellHost, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer localizer) 
            : base(shellSettingsManager, shellHost, scriptEvaluator, localizer)
        {
        }

        //public override async Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        //{
        //    return true;
        //}
        
        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Halt();
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Done");
        }
    }
}