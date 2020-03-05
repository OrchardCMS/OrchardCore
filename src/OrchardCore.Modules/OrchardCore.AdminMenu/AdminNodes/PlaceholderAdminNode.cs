using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using OrchardCore.AdminMenu.Models;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class PlaceholderAdminNode : AdminNode
    {
        [Required]
        public string LinkText { get; set; }

        public string IconClass { get; set; }
    }
}
