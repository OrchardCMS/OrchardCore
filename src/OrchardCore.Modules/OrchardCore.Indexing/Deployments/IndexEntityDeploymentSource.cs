using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Indexing.Deployments;

internal sealed class IndexEntityDeploymentSource : DeploymentSourceBase<IndexEntityDeploymentStep>
{
    private readonly IIndexEntityStore _store;

    public IndexEntityDeploymentSource(IIndexEntityStore store)
    {
        _store = store;
    }

    protected override async Task ProcessAsync(IndexEntityDeploymentStep step, DeploymentPlanResult result)
    {
        var dataSources = await _store.GetAllAsync();

        var sourceObjects = new JsonArray();

        var sourceIds = step.IncludeAll
            ? []
            : step.SourceIds ?? [];

        foreach (var dataSource in dataSources)
        {
            if (sourceIds.Length > 0 && !sourceIds.Contains(dataSource.Id))
            {
                continue;
            }

            var sourceObject = new JsonObject()
            {
                { "Id", dataSource.Id },
                { "ProviderName", dataSource.ProviderName },
                { "Type", dataSource.Type },
                { "DisplayText", dataSource.DisplayText },
                { "CreatedUtc", dataSource.CreatedUtc },
                { "OwnerId", dataSource.OwnerId },
                { "Author", dataSource.Author },
                { "Properties", dataSource.Properties?.DeepClone() },
            };

            sourceObjects.Add(sourceObject);
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = step.Name,
            ["Indexes"] = sourceObjects,
        });
    }
}
