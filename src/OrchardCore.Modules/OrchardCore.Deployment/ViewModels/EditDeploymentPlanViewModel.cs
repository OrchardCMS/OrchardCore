using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Deployment.ViewModels
{
    public class EditDeploymentPlanViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
