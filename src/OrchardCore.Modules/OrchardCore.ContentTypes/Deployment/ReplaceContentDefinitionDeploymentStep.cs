using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment
{
    public class ReplaceContentDefinitionDeploymentStep : DeploymentStep
    {
        public ReplaceContentDefinitionDeploymentStep()
        {
            Name = "ReplaceContentDefinition";
        }

        public bool IncludeAll { get; set; }

        public string[] ContentTypes { get; set; }

        public string[] ContentParts { get; set; }
    }
}
