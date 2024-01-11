using System;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment
{
    /// <summary>
    /// Deletes selected content definitions to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class DeleteContentDefinitionDeploymentStep : DeploymentStep
    {
        public DeleteContentDefinitionDeploymentStep()
        {
            Name = "DeleteContentDefinition";
        }

        public string[] ContentTypes { get; set; } = Array.Empty<string>();

        public string[] ContentParts { get; set; } = Array.Empty<string>();
    }
}
