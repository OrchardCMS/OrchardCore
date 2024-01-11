using OrchardCore.Deployment;

namespace OrchardCore.Templates.Deployment
{
    /// <summary>
    /// Adds templates to a <see cref="DeploymentPlanResult"/>.
    /// </summary>
    public class AllTemplatesDeploymentStep : DeploymentStep
    {
        public AllTemplatesDeploymentStep()
        {
            Name = "AllTemplates";
        }
        public bool ExportAsFiles { get; set; }
    }
}
