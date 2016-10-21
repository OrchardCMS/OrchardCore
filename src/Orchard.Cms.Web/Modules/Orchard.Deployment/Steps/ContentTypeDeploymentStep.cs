using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Deployment.Steps
{
    /// <summary>
    /// Adds all content items of a specific type to a <see cref="DeploymentPlanResult"/>. 
    /// </summary>
    public class ContentTypeDeploymentStep : DeploymentStep
    {
        public ContentTypeDeploymentStep()
        {
            Name = "ContentTypeDeploymentStep";
        }

        public string ContentType { get; set; }
    }
}
