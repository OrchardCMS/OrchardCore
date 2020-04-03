using System.ComponentModel.DataAnnotations;

namespace OrchardCore.AdminMenu.ViewModels
{
    public class AdminMenuCreateViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}
