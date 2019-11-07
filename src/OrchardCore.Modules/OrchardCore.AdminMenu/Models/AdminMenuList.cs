using System.Collections.Generic;

namespace OrchardCore.AdminMenu.Models
{
    /// <summary>
    /// The list of all the AdminMenu stored on the system.
    /// </summary>
    public class AdminMenuList
    {
        public List<AdminMenu> AdminMenu { get; set; } = new List<AdminMenu>();
    }
}
