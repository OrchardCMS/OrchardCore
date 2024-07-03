using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;
using OrchardCore.Queries.Indexes;
using YesSql;

namespace OrchardCore.Queries.Deployment
{
    public class QueryBasedContentDeploymentSource : IDeploymentSource
    {
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;

        public QueryBasedContentDeploymentSource(
            ISession session,
            IServiceProvider serviceProvider)
        {
            _session = session;
            _serviceProvider = serviceProvider;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var queryDeploymentStep = step as QueryBasedContentDeploymentStep;

            if (queryDeploymentStep == null)
            {
                return;
            }

            var data = new JsonArray();

            var query = await _session.Query<Query, QueryIndex>(q => q.Name == queryDeploymentStep.QueryName).FirstOrDefaultAsync();

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

            var querySource = _serviceProvider.GetRequiredKeyedService<IQuerySource>(query.Source);

            var results = await querySource.ExecuteQueryAsync(query, parameters);

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
}
