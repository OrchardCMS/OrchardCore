using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment
{
    public class ContentDefinitionDeploymentSource : IDeploymentSource
    {
        private readonly IContentDefinitionStore _contentDefinitionStore;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ContentDefinitionDeploymentSource(
            IContentDefinitionStore contentDefinitionStore,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _contentDefinitionStore = contentDefinitionStore;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
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

            result.Steps.Add(new JsonObject
            {
                ["name"] = "ContentDefinition",
                ["ContentTypes"] = JArray.FromObject(contentTypes, _jsonSerializerOptions),
                ["ContentParts"] = JArray.FromObject(contentParts, _jsonSerializerOptions),
            });
        }
    }
}
