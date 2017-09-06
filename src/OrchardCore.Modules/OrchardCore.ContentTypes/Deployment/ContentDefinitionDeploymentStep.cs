using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment
{
    /// <summary>
    /// Adds all content definitions to a <see cref="DeploymentPlanResult"/>. 
    /// </summary>
    public class ContentDefinitionDeploymentStep : DeploymentStep
    {
        public ContentDefinitionDeploymentStep()
        {
            Name = "ContentDefinition";
        }
    }
}
