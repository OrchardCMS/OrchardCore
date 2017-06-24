using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Environment.Shell;

namespace Orchard.Tenants.ViewModels
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

        [BindNever]
        public ShellSettings ShellSettings { get; set; }
    }

}
