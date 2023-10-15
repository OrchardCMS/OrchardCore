using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan
{
    public class ContentItemDeploymentSource : IDeploymentSource
    {
        private readonly IContentManager _contentManager;

        public ContentItemDeploymentSource(IContentManager contentManager)
        {
            _contentManager = contentManager;
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

            var jContentItem = JsonSerializer.SerializeToNode(contentItem);
            jContentItem.Remove(nameof(ContentItem.Id));

            var contentStep = result.Steps.FirstOrDefault(s => s["name"]?.ToString() == "Content");
            if (contentStep != null)
            {
                contentStep["data"]?.AsArray().Add(jContentItem);
            }
            else
            {
                result.Steps.Add(new JObject(
                    new JProperty("name", "Content"),
                    new JProperty("data", new JArray()
                    {
                        jContentItem
                    })
                ));
            }
        }
    }
}
