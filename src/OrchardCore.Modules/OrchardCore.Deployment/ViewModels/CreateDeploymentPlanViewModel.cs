using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Deployment.ViewModels
{
    public class CreateDeploymentPlanViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}
