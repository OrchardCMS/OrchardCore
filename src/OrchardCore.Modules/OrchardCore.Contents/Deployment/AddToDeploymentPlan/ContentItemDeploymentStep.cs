using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan
{
    /// <summary>
    /// Adds a content item to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class ContentItemDeploymentStep : DeploymentStep
    {
        public ContentItemDeploymentStep()
        {
            Name = nameof(ContentItemDeploymentStep);
        }

        public JObject ContentItem { get; set; }
    }
}
