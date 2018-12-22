using System;
using System.Collections.Generic;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu.Models
{
    /// <summary>
    /// The list of all the AdminMenu stored on the system.
    /// </summary>
    public class AdminTreeList
    {
        public int Id { get; set; }
        public List<AdminTree> AdminMenu { get; set; } = new List<AdminTree>();
    }
}
