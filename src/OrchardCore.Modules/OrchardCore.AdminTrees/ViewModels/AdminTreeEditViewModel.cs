using System.ComponentModel.DataAnnotations;

namespace OrchardCore.AdminTrees.ViewModels
{
    public class AdminTreeEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
}
