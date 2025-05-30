using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.Deployment;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Deployment;

public sealed class AzureAISearchIndexDeploymentSource
    : DeploymentSourceBase<AzureAISearchIndexDeploymentStep>
{
    private readonly IIndexEntityStore _indexStore;
    private readonly IEnumerable<IIndexEntityHandler> _handlers;
    private readonly ILogger _logger;

    public AzureAISearchIndexDeploymentSource(
        IIndexEntityStore indexStore,
        IEnumerable<IIndexEntityHandler> handlers,
        ILogger<AzureAISearchIndexDeploymentSource> logger)
    {
        _indexStore = indexStore;
        _handlers = handlers;
        _logger = logger;
    }

    protected override async Task ProcessAsync(AzureAISearchIndexDeploymentStep step, DeploymentPlanResult result)
    {
        var indexes = await _indexStore.GetAsync(AzureAISearchConstants.ProviderName);

        var data = new JsonArray();

        var indicesToAdd = step.IncludeAll
            ? indexes.Select(x => x.IndexName).ToArray()
            : step.IndexNames;

        foreach (var index in indexes)
        {
            if (index.IndexName == null || !indicesToAdd.Contains(index.IndexName))
            {
                continue;
            }

            var metadata = index.As<ContentIndexMetadata>();

            var contentTypes = new JsonArray();

            if (metadata.IndexedContentTypes != null)
            {
                foreach (var contentType in metadata.IndexedContentTypes)
                {
                    contentTypes.Add(contentType);
                }
            }

            var indexMetadata = index.As<AzureAISearchIndexMetadata>();

            // indexMetadata.IndexMappings
            var indexInfo = new JsonObject()
            {
                { "ProviderName", AzureAISearchConstants.ProviderName },
                { "Type", index.Type },
                { "DisplayText", index.DisplayText },
                { "AnalyzerName", indexMetadata.AnalyzerName },
                { "QueryAnalyzerName", index.As<AzureAISearchDefaultQueryMetadata>().QueryAnalyzerName },
                { "IndexMappings", JArray.FromObject(indexMetadata.IndexMappings ?? []) },
                { "IndexedContentTypes", contentTypes },
                { "Culture", metadata.Culture },
                { "IndexLatest", metadata.IndexLatest },
                { "Properties", index.Properties?.DeepClone() },
            };

            var exportingContext = new IndexEntityExportingContext(index, indexInfo);

            await _handlers.InvokeAsync((handler, context) => handler.ExportingAsync(context), exportingContext, _logger);

            data.Add(indexInfo);
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = step.Name,
            ["Indices"] = data,
        });
    }
}
