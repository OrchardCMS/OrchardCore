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
        public EnableTenantTask(IShellSettingsManager shellSettingsManager, IShellHost shellHost, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<EnableTenantTask> localizer) 
            : base(shellSettingsManager, shellHost, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(EnableTenantTask);
        public override LocalizedString Category => T["Tenant"];
        public override LocalizedString DisplayText => T["Enable Tenant Task"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Enabled"]);
        }
    }
}