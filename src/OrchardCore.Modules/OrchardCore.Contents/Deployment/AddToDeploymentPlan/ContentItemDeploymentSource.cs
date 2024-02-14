using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan
{
    public class ContentItemDeploymentSource : IDeploymentSource
    {
        private readonly IContentManager _contentManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ContentItemDeploymentSource(
            IContentManager contentManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _contentManager = contentManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var contentItemDeploymentStep = step as ContentItemDeploymentStep;

            if (contentItemDeploymentStep == null || contentItemDeploymentStep.ContentItemId == null)
            {
                return;
            }

            var contentItem = await _contentManager.GetAsync(contentItemDeploymentStep.ContentItemId);

            if (contentItem == null)
            {
                return;
            }

            var jContentItem = JObject.FromObject(contentItem, _jsonSerializerOptions);
            jContentItem.Remove(nameof(ContentItem.Id));

            var contentStep = result.Steps.FirstOrDefault(s => s["name"]?.ToString() == "Content");
            if (contentStep != null)
            {
                var data = contentStep["data"] as JsonArray;
                data.Add(jContentItem);
            }
            else
            {
                result.Steps.Add(new JsonObject
                {
                    ["name"] = "Content",
                    ["data"] = new JsonArray(jContentItem),
                });
            }
        }
    }
}
