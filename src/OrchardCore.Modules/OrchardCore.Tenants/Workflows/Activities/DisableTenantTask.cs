using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities
{
    public class DisableTenantTask : TenantTask
    {
        public DisableTenantTask(IShellSettingsManager shellSettingsManager, IShellHost shellHost, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<DisableTenantTask> localizer) 
            : base(shellSettingsManager, shellHost, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(DisableTenantTask);
        public override LocalizedString Category => T["Tenant"];
        public override LocalizedString DisplayText => T["Disable Tenant Task"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Disabled"]);
        }

        //public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        //{
        //    //var shellSettings = await GetTenantAsync(workflowContext);
        //    //shellSettings.State = TenantState.Disabled;
        //    //await ShellHost.UpdateShellSettingsAsync(shellSettings);

        //    return Outcomes("Disable");
        //}
    }
}