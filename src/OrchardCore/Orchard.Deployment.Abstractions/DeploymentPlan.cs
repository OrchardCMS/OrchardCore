using System.Collections.Generic;

namespace Orchard.Deployment
{
    /// <summary>
    /// The state of a deployment plan built by sources.
    /// </summary>
    public class DeploymentPlan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<DeploymentStep> DeploymentSteps { get; } = new List<DeploymentStep>();
    }
}
