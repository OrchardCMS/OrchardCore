using System.ComponentModel.DataAnnotations;

namespace Orchard.Deployment.Steps
{
    /// <summary>
    /// Adds all content items of a specific type to a <see cref="DeploymentPlanResult"/>. 
    /// </summary>
    public class ContentTypeDeploymentStep : DeploymentStep
    {
        public ContentTypeDeploymentStep()
        {
            Name = "ContentTypeDeploymentStep";
        }

        public string[] ContentTypes { get; set; }
    }
}
