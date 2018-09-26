using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Deployment;

namespace OrchardCore.Workflows.Deployment
{
    public class AllWorkflowTypeDeploymentStep : DeploymentStep
    {
        public AllWorkflowTypeDeploymentStep()
        {
            Name = "AllWorkflowType";
        }
    }
}
