using System.Collections.Generic;

namespace OrchardCore.Tenant
{
    public class SingleTenantSettingsManager : ITenantSettingsManager
    {
        public IEnumerable<TenantSettings> LoadSettings()
        {
            yield return new TenantSettings
            {
                Name = "Default",
                State = Models.TenantState.Running
            };
        }

        public void SaveSettings(TenantSettings tenantSettings)
        {
        }
    }
}