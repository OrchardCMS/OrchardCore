using System.ComponentModel.DataAnnotations;

namespace Orchard.Deployment.ViewModels
{
    public class CreateDeploymentPlanViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}
