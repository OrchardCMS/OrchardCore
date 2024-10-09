using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Deployment;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Deployment;

public class ContentDeploymentSource
    : DeploymentSourceBase<ContentDeploymentStep>
{
    private readonly ISession _session;

    public ContentDeploymentSource(ISession session)
    {
        _session = session;
    }

    protected override async Task ProcessAsync(ContentDeploymentStep step, DeploymentPlanResult result)
    {
        // TODO: Batch and create separate content files in the result.
        var data = new JsonArray();

        foreach (var contentItem in await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.ContentType.IsIn(step.ContentTypes)).ListAsync())
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
