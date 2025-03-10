using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Handlers;

public sealed class AzureAISearchIndexHandler : AzureAISearchIndexSettingsHandlerBase
{
    private readonly AzureAISearchIndexNameService _searchIndexNameService;
    private readonly IStringLocalizer S;

    public AzureAISearchIndexHandler(
        AzureAISearchIndexNameService searchIndexNameService,
        IStringLocalizer<AzureAISearchIndexHandler> stringLocalizer)
    {
        _searchIndexNameService = searchIndexNameService;
        S = stringLocalizer;
    }

    public override Task CreatingAsync(AzureAISearchIndexSettingsCreateContext context)
    {
        context.Settings.IndexFullName = _searchIndexNameService.GetFullIndexName(context.Settings.IndexName);

        return Task.CompletedTask;
    }

    public override Task InitializingAsync(AzureAISearchIndexSettingsInitializingContext context)
        => PopulateAsync(context.Settings, context.Data);

    private static Task PopulateAsync(AzureAISearchIndexSettings index, JsonNode data)
    {
        var name = data[nameof(AzureAISearchIndexSettings.IndexName)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(name))
        {
            index.IndexName = name;
        }

        var analyzerName = data[nameof(AzureAISearchIndexSettings.AnalyzerName)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(analyzerName))
        {
            index.AnalyzerName = analyzerName;
        }

        var queryAnalyzerName = data[nameof(AzureAISearchIndexSettings.QueryAnalyzerName)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(queryAnalyzerName))
        {
            index.QueryAnalyzerName = queryAnalyzerName;
        }

        return Task.CompletedTask;
    }

    public override Task ValidatingAsync(AzureAISearchIndexSettingsValidatingContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Settings.IndexName))
        {
            context.Result.Fail(new ValidationResult(S["The index name is required."]));
        }
        else if (!AzureAISearchIndexNamingHelper.TryGetSafeIndexName(context.Settings.IndexName, out var indexName) || indexName != context.Settings.IndexName)
        {
            context.Result.Fail(new ValidationResult(S["The index name contains forbidden characters."]));
        }

        return Task.CompletedTask;
    }
}
