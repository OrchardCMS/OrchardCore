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
            if (!(step is ContentDefinitionDeploymentStep contentDefinitionStep))
            {
                return;
            }

            var contentTypeDefinitionDocument = await _contentDefinitionStore.LoadContentDefinitionAsync();
           
            var contentTypes = contentDefinitionStep.IncludeAll
                ? contentTypeDefinitionDocument.ContentTypeDefinitions
                : contentTypeDefinitionDocument.ContentTypeDefinitions
                    .Where(x => contentDefinitionStep.ContentTypes.Contains(x.Name));

            var contentParts = contentDefinitionStep.IncludeAll
                ? contentTypeDefinitionDocument.ContentPartDefinitions
                : contentTypeDefinitionDocument.ContentPartDefinitions
                        .Where(x => contentDefinitionStep.ContentParts.Contains(x.Name));

            result.Steps.Add(new JObject(
                new JProperty("name", "ContentDefinition"),
                new JProperty("ContentTypes", JArray.FromObject(contentTypes)),
                new JProperty("ContentParts", JArray.FromObject(contentParts))
            ));

            return;
        }
    }
}
