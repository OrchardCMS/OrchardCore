using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment
{
    public class DeleteContentDefinitionDeploymentSource : IDeploymentSource
    {
        public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not DeleteContentDefinitionDeploymentStep deleteContentDefinitionStep)
            {
                return Task.CompletedTask;
            }

            result.Steps.Add(new JObject(
                new JProperty("name", "DeleteContentDefinition"),
                new JProperty("ContentTypes", JArray.FromObject(deleteContentDefinitionStep.ContentTypes)),
                new JProperty("ContentParts", JArray.FromObject(deleteContentDefinitionStep.ContentParts))
            ));

            return Task.CompletedTask;
        }
    }
}
