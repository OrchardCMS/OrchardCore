using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;

namespace OrchardCore.Queries.Deployment
{
    public class QueryBasedContentDeploymentSource : IDeploymentSource
    {
        private readonly IQueryManager _queryManager;

        public QueryBasedContentDeploymentSource(IQueryManager queryManager)
        {
            _queryManager = queryManager;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var queryDeploymentStep = step as QueryBasedContentDeploymentStep;

            if (queryDeploymentStep == null)
            {
                return;
            }

            var data = new JArray();

            var query = await _queryManager.GetQueryAsync(queryDeploymentStep.QueryName);

            if (query == null)
            {
                return;
            }

            if (!query.ResultsOfType<ContentItem>())
            {
                return;
            }

            if (!TryDeserializeParameters(queryDeploymentStep.QueryParameters ?? "{ }", out var parameters))
            {
                return;
            }

            var results = await _queryManager.ExecuteQueryAsync(query, parameters);

            foreach (var contentItem in results.Items)
            {
                var objectData = JObject.FromObject(contentItem);

                // Don't serialize the Id as it could be interpreted as an updated object when added back to YesSql.
                objectData.Remove(nameof(ContentItem.Id));

                if (queryDeploymentStep.ExportAsSetupRecipe)
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

            if (data.HasValues)
            {
                var jobj = new JObject
                {
                    ["name"] = "content",
                    ["data"] = data
                };

                result.Steps.Add(jobj);
            }
        }

        private static bool TryDeserializeParameters(string parameters, out Dictionary<string, object> queryParameters)
        {
            try
            {
                queryParameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters) ?? new();
                return true;
            }
            catch (JsonException)
            {
                queryParameters = new();
                return false;
            }
        }
    }
}
