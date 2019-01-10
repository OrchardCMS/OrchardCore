using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Tenants.ViewModels
{
    public class AddTenantViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}
