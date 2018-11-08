using System.ComponentModel.DataAnnotations;

namespace OrchardCore.AdminTrees.ViewModels
{
    public class AdminTreeEditViewModel
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
