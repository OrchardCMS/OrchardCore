using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment
{
    public class ReplaceContentDefinitionDeploymentSource : IDeploymentSource
    {
        private readonly IContentDefinitionStore _contentDefinitionStore;

        public ReplaceContentDefinitionDeploymentSource(IContentDefinitionStore contentDefinitionStore)
        {
            _contentDefinitionStore = contentDefinitionStore;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not ReplaceContentDefinitionDeploymentStep replaceContentDefinitionStep)
            {
                return;
            }

            var contentTypeDefinitionRecord = await _contentDefinitionStore.LoadContentDefinitionAsync();

            var contentTypes = replaceContentDefinitionStep.IncludeAll
                ? contentTypeDefinitionRecord.ContentTypeDefinitionRecords
                : contentTypeDefinitionRecord.ContentTypeDefinitionRecords
                    .Where(x => replaceContentDefinitionStep.ContentTypes.Contains(x.Name));

            var contentParts = replaceContentDefinitionStep.IncludeAll
                ? contentTypeDefinitionRecord.ContentPartDefinitionRecords
                : contentTypeDefinitionRecord.ContentPartDefinitionRecords
                        .Where(x => replaceContentDefinitionStep.ContentParts.Contains(x.Name));

            result.Steps.Add(new JObject(
                new JProperty("name", "ReplaceContentDefinition"),
                new JProperty("ContentTypes", JArray.FromObject(contentTypes)),
                new JProperty("ContentParts", JArray.FromObject(contentParts))
            ));
        }
    }
}
