using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Deployment;
using YesSql;

namespace OrchardCore.Contents.Deployment
{
    public class AllContentDeploymentSource : IDeploymentSource
    {
        private readonly ISession _session;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AllContentDeploymentSource(
            ISession session,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _session = session;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allContentStep = step as AllContentDeploymentStep;

            if (allContentStep == null)
            {
                return;
            }

            var data = new JsonArray();
            result.Steps.Add(new JsonObject
            {
                ["name"] = "Content",
                ["data"] = data,
            });

            foreach (var contentItem in await _session.Query<ContentItem, ContentItemIndex>(x => x.Published).ListAsync())
            {
                var objectData = JObject.FromObject(contentItem, _jsonSerializerOptions);

                // Don't serialize the Id as it could be interpreted as an updated object when added back to YesSql
                objectData.Remove(nameof(ContentItem.Id));

                if (allContentStep.ExportAsSetupRecipe)
                {
                    objectData[nameof(ContentItem.Owner)] = "[js: parameters('AdminUserId')]";
                    objectData[nameof(ContentItem.Author)] = "[js: parameters('AdminUsername')]";
                    objectData[nameof(ContentItem.ContentItemId)] = "[js: uuid()]";
                    objectData.Remove(nameof(ContentItem.ContentItemVersionId));
                    objectData.Remove(nameof(ContentItem.CreatedUtc));
                    objectData.Remove(nameof(ContentItem.ModifiedUtc));
                    objectData.Remove(nameof(ContentItem.PublishedUtc));
                }

                data.Add(objectData);
            }

            return;
        }
    }
}
