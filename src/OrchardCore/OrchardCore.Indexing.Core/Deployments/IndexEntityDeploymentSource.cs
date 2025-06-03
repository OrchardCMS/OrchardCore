using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.Deployment;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class IndexEntityDeploymentSource : DeploymentSourceBase<IndexEntityDeploymentStep>
{
    private readonly IIndexEntityStore _store;
    private readonly IEnumerable<IIndexEntityHandler> _handlers;
    private readonly ILogger _logger;

    public IndexEntityDeploymentSource(
        IIndexEntityStore store,
        IEnumerable<IIndexEntityHandler> handlers,
        ILogger<IndexEntityDeploymentSource> logger)
    {
        _store = store;
        _handlers = handlers;
        _logger = logger;
    }

    protected override async Task ProcessAsync(IndexEntityDeploymentStep step, DeploymentPlanResult result)
    {
        var indexes = await _store.GetAllAsync();

        var indexObjects = new JsonArray();

        var sourceIds = step.IncludeAll
            ? []
            : step.IndexeIds ?? [];

        foreach (var index in indexes)
        {
            if (sourceIds.Length > 0 && !sourceIds.Contains(index.Id))
            {
                continue;
            }

            var indexInfo = new JsonObject()
            {
                { "Id", index.Id },
                { "ProviderName", index.ProviderName },
                { "Type", index.Type },
                { "DisplayText", index.Name },
                { "CreatedUtc", index.CreatedUtc },
                { "OwnerId", index.OwnerId },
                { "Author", index.Author },
                { "Properties", index.Properties?.DeepClone() },
            };

            var exportingContext = new IndexEntityExportingContext(index, indexInfo);

            await _handlers.InvokeAsync((handler, context) => handler.ExportingAsync(context), exportingContext, _logger);

            indexObjects.Add(indexInfo);
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = step.Name,
            ["Indexes"] = indexObjects,
        });
    }
}
