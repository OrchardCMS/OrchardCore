using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Deployment.Steps
{
    /// <summary>
    /// Adds a custom file to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class CustomFileDeploymentStep : DeploymentStep
    {
        public CustomFileDeploymentStep()
        {
            Name = nameof(CustomFileDeploymentStep);
        }

        [Required]
        public string FileName { get; set; }

        public string FileContent { get; set; }
    }
}
