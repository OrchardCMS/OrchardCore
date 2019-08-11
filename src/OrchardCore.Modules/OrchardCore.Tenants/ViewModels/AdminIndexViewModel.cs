using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tenants.ViewModels
{
    public class AdminIndexViewModel
    {
        public List<ShellSettingsEntry> ShellSettingsEntries { get; set; } = new List<ShellSettingsEntry>();
        public TenantIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class BulkActionViewModel
    {
        public BulkAction BulkAction { get; set; }
        public string[] TenantNames{ get; set; }
    }

    public enum BulkAction
    {
        Disable,
        Enable
    }

    public class ShellSettingsEntry
    {
        public bool Selected { get; set; }
        public string Name { get; set; }
        public bool IsDefaultTenant { get; set; }

        public string Token { get; set; }
        [BindNever]
        public ShellSettings ShellSettings { get; set; }
    }
    
    public class TenantIndexOptions
    {
        public string Search { get; set; }
        public TenantsFilter Filter { get; set; }
        public TenantsBulkAction BulkAction { get; set; }
        public TenantsOrder OrderBy { get; set; }
    }

    public enum TenantsFilter
    {
        All,
        Running,
        Disabled,
        Uninitialized
    }

    public enum TenantsBulkAction
    {
        None,
        Disable
    }

    public enum TenantsOrder
    {
        Name,
        State
    }
}
