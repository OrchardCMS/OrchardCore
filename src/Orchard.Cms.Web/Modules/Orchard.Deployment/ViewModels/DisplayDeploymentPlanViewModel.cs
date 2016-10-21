using System.Collections.Generic;

namespace Orchard.Deployment.ViewModels
{
    public class DisplayDeploymentPlanViewModel
    {
        public DeploymentPlan DeploymentPlan { get; set; }
        public IEnumerable<dynamic> Items { get; set; }
        public IDictionary<string, dynamic> Thumbnails { get; set; }
    }
}
