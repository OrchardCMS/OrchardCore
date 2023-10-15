using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment
{
    public class DeleteContentDefinitionDeploymentSource : IDeploymentSource
    {
        public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is DeleteContentDefinitionDeploymentStep deleteContentDefinitionStep)
            {
                ContentDefinitionDeploymentSource.AddContentTypeAndPartStep(
                    result,
                    "DeleteContentDefinition",
                    deleteContentDefinitionStep.ContentTypes,
                    deleteContentDefinitionStep.ContentParts);
            }

            return Task.CompletedTask;
        }
    }
}
