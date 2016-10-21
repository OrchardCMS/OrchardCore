using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Deployment.Steps
{
    /// <summary>
    /// Adds all content items to a <see cref="DeploymentPlanResult"/>. 
    /// </summary>
    public class AllContentDeploymentStep : DeploymentStep
    {
        public AllContentDeploymentStep()
        {
            Name = "AllContent";
        }
    }
}
