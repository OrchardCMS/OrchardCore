using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows.Activities
{
    public class TenantDisableEvent : TenantEvent
    {
        public TenantDisableEvent(IShellSettingsManager shellSettingsManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<TenantCreatedEvent> localizer) 
            : base(shellSettingsManager, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(TenantDisableEvent);
    }
}