using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public sealed class ElasticsearchIndexDeploymentSource
    : DeploymentSourceBase<ElasticsearchIndexDeploymentStep>
{
    private readonly IIndexProfileStore _indexStore;

    public ElasticsearchIndexDeploymentSource(IIndexProfileStore indexStore)
    {
        _indexStore = indexStore;
    }

    protected override async Task ProcessAsync(ElasticsearchIndexDeploymentStep step, DeploymentPlanResult result)
    {
        var indexSettings = await _indexStore.GetByProviderAsync(ElasticsearchConstants.ProviderName).ConfigureAwait(false);

        var data = new JsonArray();
        var indicesToAdd = step.IncludeAll
            ? indexSettings.Select(x => x.IndexName).ToArray()
            : step.IndexNames;

        foreach (var index in indexSettings)
        {
            if (indicesToAdd.Contains(index.IndexName))
            {
                var indexSettingsDict = new Dictionary<string, IndexProfile>
                {
                    { index.IndexName, index },
                };

                data.Add(JObject.FromObject(indexSettingsDict));
            }
        }

        // Adding Elasticsearch settings.
        result.Steps.Add(new JsonObject
        {
            ["name"] = "ElasticIndexSettings",
            ["Indices"] = data,
        });
    }
}
