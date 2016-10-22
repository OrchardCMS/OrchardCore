using System.Collections.Generic;

namespace Orchard.Deployment
{
    public interface IDeploymentTargetProvider
    {
        List<DeploymentTarget> DeploymentTargets { get; }
    }
}
