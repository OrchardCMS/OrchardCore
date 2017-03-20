using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Tenant;

namespace Orchard.Tenants.ViewModels
{
    public class AdminIndexViewModel
    {
        public List<TenantSettingsEntry> TenantSettingsEntries { get; set; } = new List<TenantSettingsEntry>();
    }

    public class TenantSettingsEntry
    {
        public bool Selected { get; set; }
        public string Name { get; set; }
        public bool IsDefaultTenant { get; set; }

        [BindNever]
        public TenantSettings TenantSettings { get; set; }
    }

}
