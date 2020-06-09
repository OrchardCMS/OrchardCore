using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment
{
    /// <summary>
    /// Adds all content items to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class AllContentDeploymentStep : DeploymentStep
    {
        public AllContentDeploymentStep()
        {
            Name = "AllContent";
        }

        public bool ExportAsSetupRecipe { get; set; }
    }
}
