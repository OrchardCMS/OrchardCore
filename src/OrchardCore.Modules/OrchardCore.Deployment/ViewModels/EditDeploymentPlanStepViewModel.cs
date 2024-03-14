using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Deployment.ViewModels
{
    public class EditDeploymentPlanStepViewModel
    {
        public long DeploymentPlanId { get; set; }
        public string DeploymentStepId { get; set; }
        public string DeploymentStepType { get; set; }
        public dynamic Editor { get; set; }

        [BindNever]
        public DeploymentStep DeploymentStep { get; set; }
    }
}
