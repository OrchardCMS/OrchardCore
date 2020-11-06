using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using OrchardCore.AdminMenu.Models;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class LinkAdminNode : AdminNode
    {
        [Required]
        public string LinkText { get; set; }

        [Required]
        public string LinkUrl { get; set; }

        public string IconClass { get; set; }
    }
}
