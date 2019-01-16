using System.Collections.Generic;

namespace OrchardCore.AdminMenu.ViewModels
{
    public class AdminMenuListViewModel
    {
        public IList<AdminMenuEntry> AdminMenu { get; set; }
        public AdminMenuListOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class AdminMenuEntry
    {
        public Models.AdminMenu AdminMenu { get; set; }
        public bool IsChecked { get; set; }
    }
}
