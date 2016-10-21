using System.Collections.Generic;

namespace Orchard.Deployment.ViewModels
{
    public class DisplayDeploymentPlanViewModel
    {
        public DeploymentPlan DeploymentPlan { get; set; }
        public IEnumerable<dynamic> Items { get; set; }
        public string[] DeploymentStepTypes { get; set; }
    }
}
