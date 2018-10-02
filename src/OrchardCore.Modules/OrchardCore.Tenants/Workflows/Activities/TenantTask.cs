using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities
{
    public abstract class TenantTask : TenantActivity, ITask
    {
        protected TenantTask(IShellSettingsManager shellSettingsManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer localizer) 
            : base(shellSettingsManager, scriptEvaluator, localizer)
        {
        }
    }
}