using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Search.OpenSearch.Core.Deployment;

public sealed class OpenSearchIndexDeploymentSource
    : DeploymentSourceBase<OpenSearchIndexDeploymentStep>
{
    private readonly IIndexProfileStore _indexStore;

    public OpenSearchIndexDeploymentSource(IIndexProfileStore indexStore)
    {
        _indexStore = indexStore;
    }

    protected override async Task ProcessAsync(OpenSearchIndexDeploymentStep step, DeploymentPlanResult result)
    {
        var indexSettings = await _indexStore.GetByProviderAsync(OpenSearchConstants.ProviderName);

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

        result.Steps.Add(new JsonObject
        {
            ["name"] = "OpenSearchIndexSettings",
            ["Indices"] = data,
        });
    }
}
