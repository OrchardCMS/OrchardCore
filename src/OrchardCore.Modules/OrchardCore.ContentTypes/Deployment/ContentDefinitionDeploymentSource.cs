using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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

            AddContentTypeAndPartStep(result, "ContentDefinition", contentTypes, contentParts);
        }

        public static void AddContentTypeAndPartStep(DeploymentPlanResult result, string name, IEnumerable contentTypes, IEnumerable contentParts) =>
            result.AddStep(name, new[]
            {
                new KeyValuePair<string, JsonNode>("ContentTypes", JsonSerializer.SerializeToNode(contentTypes)),
                new KeyValuePair<string, JsonNode>("ContentParts", JsonSerializer.SerializeToNode(contentParts)),
            });
    }
}
