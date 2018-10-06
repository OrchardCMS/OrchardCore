using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;

namespace OrchardCore.Tenants.Workflows.Drivers
{
    public class TenantDisabledEventDisplay : TenantEventDisplayDriver<TenantDisabledEvent, TenantDisabledEventViewModel>
    {
        public TenantDisabledEventDisplay(IShellSettingsManager shellSettingsManager) : base(shellSettingsManager)
        {
        }
    }
}
