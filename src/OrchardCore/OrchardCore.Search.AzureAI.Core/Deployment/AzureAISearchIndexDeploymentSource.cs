using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.Deployment;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Deployment;

public sealed class AzureAISearchIndexDeploymentSource
    : DeploymentSourceBase<AzureAISearchIndexDeploymentStep>
{
    private readonly AzureAISearchIndexSettingsService _indexSettingsService;
    private readonly IEnumerable<IAzureAISearchIndexSettingsHandler> _handlers;
    private readonly ILogger _logger;

    public AzureAISearchIndexDeploymentSource(
        AzureAISearchIndexSettingsService indexSettingsService,
        IEnumerable<IAzureAISearchIndexSettingsHandler> handlers,
        ILogger<AzureAISearchIndexDeploymentSource> logger)
    {
        _indexSettingsService = indexSettingsService;
        _handlers = handlers;
        _logger = logger;
    }

    protected override async Task ProcessAsync(AzureAISearchIndexDeploymentStep step, DeploymentPlanResult result)
    {
        var indexSettings = await _indexSettingsService.GetSettingsAsync();

        var data = new JsonArray();

        var indicesToAdd = step.IncludeAll
            ? indexSettings.Select(x => x.IndexName).ToArray()
            : step.IndexNames;

        foreach (var index in indexSettings)
        {
            if (index.IndexName == null || !indicesToAdd.Contains(index.IndexName))
            {
                continue;
            }

            var indexInfo = new JsonObject()
            {
                { "IndexName", index.IndexName },
                { "AnalyzerName", index.AnalyzerName },
                { "QueryAnalyzerName", index.QueryAnalyzerName },
            };

            var exportingContext = new AzureAISearchIndexSettingsExportingContext(index, indexInfo);

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
