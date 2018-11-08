using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using OrchardCore.AdminTrees.Models;

namespace OrchardCore.AdminTrees.AdminNodes
{
    public class PlaceholderAdminNode : AdminNode
    {
        [Required]
        public string LinkText { get; set; }
        public string IconClass { get; set; }
    }
}
