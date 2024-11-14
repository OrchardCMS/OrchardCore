using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.Queries.Deployment;

public class QueryBasedContentDeploymentSource
    : DeploymentSourceBase<QueryBasedContentDeploymentStep>
{
    private readonly IQueryManager _queryManager;

    public QueryBasedContentDeploymentSource(IQueryManager queryManager)
    {
        _queryManager = queryManager;
    }

    protected override async Task ProcessAsync(QueryBasedContentDeploymentStep step, DeploymentPlanResult result)
    {
        var data = new JsonArray();

        var query = await _queryManager.GetQueryAsync(step.QueryName);

        if (query == null)
        {
            return;
        }

        if (!query.CanReturnContentItems || !query.ReturnContentItems)
        {
            return;
        }

        if (!TryDeserializeParameters(step.QueryParameters ?? "{ }", out var parameters))
        {
            return;
        }

        var results = await _queryManager.ExecuteQueryAsync(query, parameters);

        foreach (var contentItem in results.Items)
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
            var jObj = new JsonObject
            {
                ["name"] = "content",
                ["data"] = data
            };

            result.Steps.Add(jObj);
        }
    }

    private static bool TryDeserializeParameters(string parameters, out Dictionary<string, object> queryParameters)
    {
        try
        {
            queryParameters = JConvert.DeserializeObject<Dictionary<string, object>>(parameters) ?? [];

            return true;
        }
        catch (JsonException)
        {
            queryParameters = [];

            return false;
        }
    }
}
