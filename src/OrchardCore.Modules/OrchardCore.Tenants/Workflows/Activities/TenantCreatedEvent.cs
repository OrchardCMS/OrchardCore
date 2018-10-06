using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities
{
    public class TenantCreatedEvent : TenantEvent
    {
        public TenantCreatedEvent(IShellSettingsManager shellSettingsManager, IShellHost shellHost, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<TenantCreatedEvent> localizer) 
            : base(shellSettingsManager, shellHost, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(TenantCreatedEvent);
    }
}