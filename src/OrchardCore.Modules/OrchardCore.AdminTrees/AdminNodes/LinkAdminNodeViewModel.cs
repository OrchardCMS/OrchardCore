using System.ComponentModel.DataAnnotations;

namespace OrchardCore.AdminTrees.AdminNodes
{
    public class LinkAdminNodeViewModel
    {
        [Required]
        public string LinkText { get; set; }

        public string LinkUrl { get; set; }

        public bool Enabled { get; set; }

        public string IconClass { get; set; }
    }
}
