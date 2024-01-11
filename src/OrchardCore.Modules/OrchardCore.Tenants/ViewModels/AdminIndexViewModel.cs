using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tenants.ViewModels
{
    public class AdminIndexViewModel
    {
        public List<ShellSettingsEntry> ShellSettingsEntries { get; set; } = new List<ShellSettingsEntry>();

        public TenantIndexOptions Options { get; set; } = new TenantIndexOptions();

        [BindNever]
        public dynamic Pager { get; set; }
    }

    public class BulkActionViewModel
    {
        public BulkAction BulkAction { get; set; }
        public string[] TenantNames { get; set; }
    }

    public enum BulkAction
    {
        Disable,
        Enable,
        Remove
    }

    public class ShellSettingsEntry
    {
        public string Category { get; set; }

        public string Description { get; set; }

        public bool Selected { get; set; }

        public string Name { get; set; }

        public string Token { get; set; }

        [BindNever]
        public ShellSettings ShellSettings { get; set; }
    }

    public class TenantIndexOptions
    {
        public string Search { get; set; }

        public string Category { get; set; }

        public TenantsState Status { get; set; }

        public TenantsBulkAction BulkAction { get; set; }

        public TenantsOrder OrderBy { get; set; }

        public List<SelectListItem> TenantsCategories { get; set; }

        [BindNever]
        public List<SelectListItem> TenantsStates { get; set; }

        [BindNever]
        public List<SelectListItem> TenantsSorts { get; set; }

        [BindNever]
        public List<SelectListItem> TenantsBulkAction { get; set; }
    }

    public enum TenantsState
    {
        All,
        Running,
        Disabled,
        Uninitialized
    }

    public enum TenantsBulkAction
    {
        None,
        Disable,
        Enable,
        Remove
    }

    public enum TenantsOrder
    {
        Name,
        State
    }
}
