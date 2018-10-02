using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;

namespace OrchardCore.Tenants.Workflows.Drivers
{
    public class TenantCreatedEventDisplay : TenantEventDisplayDriver<TenantCreatedEvent, TenantCreatedEventViewModel>
    {
        public TenantCreatedEventDisplay(IShellSettingsManager shellSettingsManager) : base(shellSettingsManager)
        {
        }
    }
}
