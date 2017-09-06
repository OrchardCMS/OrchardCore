using System.Collections.Generic;

namespace OrchardCore.Deployment.ViewModels
{
    public class DisplayDeploymentPlanViewModel
    {
        public DeploymentPlan DeploymentPlan { get; set; }
        public IEnumerable<dynamic> Items { get; set; }
        public IDictionary<string, dynamic> Thumbnails { get; set; }
    }
}
