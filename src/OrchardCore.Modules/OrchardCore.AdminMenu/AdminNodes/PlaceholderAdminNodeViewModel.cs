using System.ComponentModel.DataAnnotations;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class PlaceholderAdminNodeViewModel
    {
        [Required]
        public string LinkText { get; set; }

        public string IconClass { get; set; }
    }
}
