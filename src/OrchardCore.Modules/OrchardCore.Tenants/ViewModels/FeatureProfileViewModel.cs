using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Tenants.ViewModels
{
    public class FeatureProfileViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string FeatureRules { get; set; }
    }
}
