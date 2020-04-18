using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan
{
    public class ContentItemDeploymentSource : IDeploymentSource
    {
        public Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var contentItemDeploymentStep = step as ContentItemDeploymentStep;

            if (contentItemDeploymentStep == null || contentItemDeploymentStep.ContentItem == null)
            {
                return Task.CompletedTask;
            }

            contentItemDeploymentStep.ContentItem.Remove(nameof(ContentItem.Id));

            var contentStep = result.Steps.FirstOrDefault(s => s["name"]?.ToString() == "Content");
            if (contentStep != null)
            {
                var data = contentStep["data"] as JArray;
                data.Add(contentItemDeploymentStep.ContentItem);
            }
            else
            {
                result.Steps.Add(new JObject(
                    new JProperty("name", "Content"),
                    new JProperty("data", new JArray()
                    {
                        contentItemDeploymentStep.ContentItem
                    })
                ));
            }

            return Task.CompletedTask;
        }
    }
}
