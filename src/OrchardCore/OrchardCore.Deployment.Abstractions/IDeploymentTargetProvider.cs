using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Deployment
{
    public interface IDeploymentTargetProvider
    {
        Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync();
    }
}
