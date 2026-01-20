using System.ComponentModel.DataAnnotations;

namespace OrchardCore.AdminMenu.ViewModels
{
    public class AdminMenuEditViewModel
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
