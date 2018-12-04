using System.ComponentModel.DataAnnotations;

namespace OrchardCore.AdminTrees.AdminNodes
{
    public class LinkAdminNodeViewModel
    {
        [Required]
        public string LinkText { get; set; }

        [Required]
        public string LinkUrl { get; set; }

        public string IconClass { get; set; }
    }
}
