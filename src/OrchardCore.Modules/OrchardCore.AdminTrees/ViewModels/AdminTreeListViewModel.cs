using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.AdminTrees.Models;

namespace OrchardCore.AdminTrees.ViewModels
{
    public class AdminTreeListViewModel
    {
        public IList<AdminTreeEntry> AdminTrees { get; set; }
        public AdminTreeListOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }


    public class AdminTreeEntry
    {
        public AdminTree AdminTree { get; set; }
        public bool IsChecked { get; set; }
    }

}
