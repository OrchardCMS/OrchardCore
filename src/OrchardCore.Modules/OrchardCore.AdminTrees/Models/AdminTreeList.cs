using System;
using System.Collections.Generic;
using OrchardCore.Navigation;

namespace OrchardCore.AdminTrees.Models
{
    /// <summary>
    /// The list of all the Admintrees stored on the system.
    /// </summary>
    public class AdminTreeList
    {
        public int Id { get; set; }
        public List<AdminTree> AdminTrees { get; set; } = new List<AdminTree>();
    }
}
