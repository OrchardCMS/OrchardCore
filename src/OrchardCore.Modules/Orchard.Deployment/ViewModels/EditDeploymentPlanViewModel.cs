using System.ComponentModel.DataAnnotations;

namespace Orchard.Deployment.ViewModels
{
    public class EditDeploymentPlanViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
