using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Deployment;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Deployment
{
    public class ContentDeploymentSource : IDeploymentSource
    {
        private readonly ISession _session;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ContentDeploymentSource(
            ISession session,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _session = session;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            // TODO: Batch and create separate content files in the result.

            var contentStep = step as ContentDeploymentStep;

            if (contentStep == null)
            {
                return;
            }

            var data = new JsonArray();

            foreach (var contentItem in await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.ContentType.IsIn(contentStep.ContentTypes)).ListAsync())
            {
                var objectData = JObject.FromObject(contentItem, _jsonSerializerOptions);

                // Don't serialize the Id as it could be interpreted as an updated object when added back to YesSql.
                objectData.Remove(nameof(ContentItem.Id));

                if (contentStep.ExportAsSetupRecipe)
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

            if (data.HasValues())
            {
                var jobj = new JsonObject
                {
                    ["name"] = "content",
                    ["data"] = data,
                };

                result.Steps.Add(jobj);
            }
        }
    }
}
