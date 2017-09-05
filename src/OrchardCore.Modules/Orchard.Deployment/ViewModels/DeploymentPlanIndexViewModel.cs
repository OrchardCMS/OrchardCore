using System.Collections.Generic;
using OrchardCore.Deployment;

namespace OrchardCore.Deployment.ViewModels
{
    public class DeploymentPlanIndexViewModel
    {
        public IList<DeploymentPlanEntry> DeploymentPlans { get; set; }
        public DeploymentPlanIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class DeploymentPlanEntry
    {
        public DeploymentPlan DeploymentPlan { get; set; }
        public bool IsChecked { get; set; }
    }

    public class DeploymentPlanIndexOptions
    {
        public string Search { get; set; }
    }

}
