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
        public DisableTenantTask(IShellSettingsManager shellSettingsManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<DisableTenantTask> localizer) 
            : base(shellSettingsManager, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(DisableTenantTask);
        public override LocalizedString Category => T["Tenant"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Deleted"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var shellSettings = await GetTenantAsync(workflowContext);
            shellSettings.State = TenantState.Disabled;
            //await _orchardHost.UpdateShellSettingsAsync(shellSettings);

            return Outcomes("Disable");
        }
    }
}