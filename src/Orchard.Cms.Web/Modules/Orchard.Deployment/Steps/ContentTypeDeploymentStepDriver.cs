using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Deployment.Editors;
using Orchard.DisplayManagement.Views;

namespace Orchard.Deployment.Steps
{
    public class ContentTypeDeploymentStepDriver : DeploymentStepDisplayDriver<ContentTypeDeploymentStep>
    {
        public override IDisplayResult Display(ContentTypeDeploymentStep step)
        {
            return Shape("ContentTypeDeploymentStep", step).Location("Content");
        }

        public override IDisplayResult Edit(ContentTypeDeploymentStep step)
        {
            return Shape("ContentTypeDeploymentStep_Edit", step).Location("Content");
        }
    }
}
