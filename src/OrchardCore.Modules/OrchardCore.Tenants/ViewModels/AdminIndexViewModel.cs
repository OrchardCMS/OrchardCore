using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tenants.ViewModels
{
    public class AdminIndexViewModel
    {
        public List<ShellSettingsEntry> ShellSettingsEntries { get; set; } = new List<ShellSettingsEntry>();
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

}
