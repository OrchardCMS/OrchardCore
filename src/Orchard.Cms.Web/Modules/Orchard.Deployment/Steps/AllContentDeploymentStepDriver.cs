using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Deployment.Editors;
using Orchard.DisplayManagement.Views;

namespace Orchard.Deployment.Steps
{
    public class AllContentDeploymentStepDriver : DeploymentStepDisplayDriver<AllContentDeploymentStep>
    {
        public override IDisplayResult Display(AllContentDeploymentStep step)
        {
            return Shape("AllContentDeploymentStep", step).Location("Content");
        }

        public override IDisplayResult Edit(AllContentDeploymentStep step)
        {
            return Shape("AllContentDeploymentStep_Edit", step).Location("Content");
        }
    }
}
