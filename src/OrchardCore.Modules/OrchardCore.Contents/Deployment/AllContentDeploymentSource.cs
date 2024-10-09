using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Deployment;
using YesSql;

namespace OrchardCore.Contents.Deployment;

public class AllContentDeploymentSource
    : DeploymentSourceBase<AllContentDeploymentStep>
{
    private readonly ISession _session;

    public AllContentDeploymentSource(ISession session)
    {
        _session = session;
    }

    protected override async Task ProcessAsync(AllContentDeploymentStep step, DeploymentPlanResult result)
    {
        var data = new JsonArray();
        result.Steps.Add(new JsonObject
        {
            ["name"] = "Content",
            ["data"] = data,
        });

        foreach (var contentItem in await _session.Query<ContentItem, ContentItemIndex>(x => x.Published).ListAsync())
        {
            var objectData = JObject.FromObject(contentItem);

            // Don't serialize the Id as it could be interpreted as an updated object when added back to YesSql.
            objectData.Remove(nameof(ContentItem.Id));

            if (step.ExportAsSetupRecipe)
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
    }
}
