using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.AdminTrees.Models;
using OrchardCore.Navigation;

namespace OrchardCore.AdminTrees.ViewModels
{
    public class AdminNodeEditViewModel
    {
        public string AdminTreeId { get; set; }
        public string AdminNodeId { get; set; }
        public string AdminNodeType { get; set; }

        public int Priority { get; set; }
        public string Position { get; set; }

        public dynamic Editor { get; set; }

        [BindNever]
        public AdminNode AdminNode { get; set; }

    }
}
