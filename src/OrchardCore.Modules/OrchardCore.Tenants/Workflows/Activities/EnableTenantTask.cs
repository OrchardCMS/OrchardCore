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
    public class EnableTenantTask : TenantTask
    {

        public EnableTenantTask(IShellSettingsManager shellSettingsManager, IShellHost shellHost, IWorkflowExpressionEvaluator expressionEvaluator, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<EnableTenantTask> localizer) 
            : base(shellSettingsManager, shellHost, expressionEvaluator, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(EnableTenantTask);
        public override LocalizedString DisplayText => T["Enable Tenant Task"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Enabled"], T["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var tenantNameTask = ExpressionEvaluator.EvaluateAsync(TenantName, workflowContext);

            if (ShellHost.TryGetSettings(tenantNameTask.Result?.Trim(), out var shellSettings))
            {
                shellSettings.State = TenantState.Running;
                await ShellHost.UpdateShellSettingsAsync(shellSettings);
            }
            else
            {
                return Outcomes("Failed");
            }

            return Outcomes("Enabled");
        }
    }
}