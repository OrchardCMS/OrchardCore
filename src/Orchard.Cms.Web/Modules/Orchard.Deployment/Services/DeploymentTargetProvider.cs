using System.Collections.Generic;

namespace Orchard.Deployment
{
    public class DeploymentTargetProvider : IDeploymentTargetProvider
    {
        public List<DeploymentTarget> DeploymentTargets { get; } = new List<DeploymentTarget>();
    }
}
