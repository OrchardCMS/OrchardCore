using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment
{
    public class ContentDefinitionDeploymentSource : IDeploymentSource
    {
        private readonly IContentDefinitionStore _contentDefinitionStore;

        public ContentDefinitionDeploymentSource(IContentDefinitionStore contentDefinitionStore)
        {
            _contentDefinitionStore = contentDefinitionStore;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not ContentDefinitionDeploymentStep contentDefinitionStep)
            {
                return;
            }

            var contentTypeDefinitionRecord = await _contentDefinitionStore.LoadContentDefinitionAsync();

            var contentTypes = contentDefinitionStep.IncludeAll
                ? contentTypeDefinitionRecord.ContentTypeDefinitionRecords
                : contentTypeDefinitionRecord.ContentTypeDefinitionRecords
                    .Where(x => contentDefinitionStep.ContentTypes.Contains(x.Name));

            var contentParts = contentDefinitionStep.IncludeAll
                ? contentTypeDefinitionRecord.ContentPartDefinitionRecords
                : contentTypeDefinitionRecord.ContentPartDefinitionRecords
                        .Where(x => contentDefinitionStep.ContentParts.Contains(x.Name));

            result.Steps.Add(new JObject(
                new JProperty("name", "ContentDefinition"),
                new JProperty("ContentTypes", JArray.FromObject(contentTypes)),
                new JProperty("ContentParts", JArray.FromObject(contentParts))
            ));
        }
    }
}
