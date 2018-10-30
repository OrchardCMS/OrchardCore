using System.ComponentModel.DataAnnotations;

namespace OrchardCore.AdminTrees.AdminNodes
{
    public class PlaceholderAdminNodeViewModel
    {
        [Required]
        public string LinkText { get; set; }
        public string IconClass { get; set; }
    }
}
