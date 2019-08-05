using System.Collections.Immutable;

namespace OrchardCore.AdminMenu.Models
{
    /// <summary>
    /// The list of all the AdminMenu stored on the system.
    /// </summary>
    public class AdminMenuList
    {
        public int Id { get; set; }
        public ImmutableArray<AdminMenu> AdminMenu { get; set; } = ImmutableArray.Create<AdminMenu>();
    }
}
