using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan;

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

        var jContentItem = JObject.FromObject(contentItem);
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
