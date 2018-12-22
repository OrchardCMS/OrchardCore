using System.ComponentModel.DataAnnotations;

namespace OrchardCore.AdminMenu.ViewModels
{
    public class AdminTreeCreateViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}
