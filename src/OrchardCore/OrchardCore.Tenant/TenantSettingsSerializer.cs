using Microsoft.Extensions.Configuration;
using OrchardCore.Tenant.Models;
using System;

namespace OrchardCore.Tenant
{
    public static class TenantSettingsSerializer
    {
        public static TenantSettings ParseSettings(IConfigurationRoot configuration)
        {
            var tenantSettings = new TenantSettings();
            tenantSettings.Name = configuration["Name"];

            TenantState state;
            tenantSettings.State = Enum.TryParse(configuration["State"], true, out state) ? state : TenantState.Uninitialized;

            tenantSettings.RequestUrlHost = configuration["RequestUrlHost"];
            tenantSettings.RequestUrlPrefix = configuration["RequestUrlPrefix"];
            tenantSettings.ConnectionString = configuration["ConnectionString"];
            tenantSettings.TablePrefix = configuration["TablePrefix"];
            tenantSettings.DatabaseProvider = configuration["DatabaseProvider"];

            return tenantSettings;
        }
    }
}
