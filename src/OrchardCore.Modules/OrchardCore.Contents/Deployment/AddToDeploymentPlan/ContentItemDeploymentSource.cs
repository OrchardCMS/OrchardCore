using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan;

public class ContentItemDeploymentSource
    : DeploymentSourceBase<ContentItemDeploymentStep>
{
    private readonly IContentManager _contentManager;

    public ContentItemDeploymentSource(IContentManager contentManager)
    {
        _contentManager = contentManager;
    }

    protected override async Task ProcessAsync(ContentItemDeploymentStep step, DeploymentPlanResult result)
    {
        if (step.ContentItemId == null)
        {
            return;
        }

        var contentItem = await _contentManager.GetAsync(step.ContentItemId);

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
