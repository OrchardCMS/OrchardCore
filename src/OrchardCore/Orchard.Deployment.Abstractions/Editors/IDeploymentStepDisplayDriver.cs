using System;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.Deployment.Editors
{
    public interface IDeploymentStepDisplayDriver : IDisplayDriver<DeploymentStep, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>
    {
        Type Type { get; }
        DeploymentStep Create();
    }
}
