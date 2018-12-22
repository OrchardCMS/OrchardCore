using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.AdminMenu.Models;

namespace OrchardCore.AdminMenu.ViewModels
{
    public class AdminTreeListViewModel
    {
        public IList<AdminTreeEntry> AdminMenu { get; set; }
        public AdminTreeListOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }


    public class AdminTreeEntry
    {
        public AdminTree AdminTree { get; set; }
        public bool IsChecked { get; set; }
    }

}
