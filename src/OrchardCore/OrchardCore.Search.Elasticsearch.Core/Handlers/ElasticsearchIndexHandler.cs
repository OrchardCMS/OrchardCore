using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Handlers;

public sealed class ElasticsearchIndexHandler : ElasticsearchIndexSettingsHandlerBase
{
    private readonly ElasticsearchIndexNameService _searchIndexNameService;
    private readonly ElasticsearchIndexManager _indexManager;

    internal readonly IStringLocalizer S;

    public ElasticsearchIndexHandler(
        ElasticsearchIndexNameService searchIndexNameService,
        ElasticsearchIndexManager indexManager,
        IStringLocalizer<ElasticsearchIndexHandler> stringLocalizer)
    {
        _searchIndexNameService = searchIndexNameService;
        _indexManager = indexManager;
        S = stringLocalizer;
    }

    public override Task CreatingAsync(ElasticsearchIndexSettingsCreateContext context)
    {
        context.Settings.IndexFullName = _searchIndexNameService.GetFullIndexName(context.Settings.IndexName);

        return Task.CompletedTask;
    }

    public override Task InitializingAsync(ElasticsearchIndexSettingsInitializingContext context)
        => PopulateAsync(context.Settings, context.Data);

    private static Task PopulateAsync(ElasticIndexSettings index, JsonNode data)
    {
        var name = data[nameof(index.IndexName)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(name))
        {
            index.IndexName = name;
        }

        var analyzerName = data[nameof(index.AnalyzerName)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(analyzerName))
        {
            index.AnalyzerName = analyzerName;
        }

        var queryAnalyzerName = data[nameof(index.QueryAnalyzerName)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(queryAnalyzerName))
        {
            index.QueryAnalyzerName = queryAnalyzerName;
        }

        return Task.CompletedTask;
    }

    public override Task ResetAsync(ElasticsearchIndexSettingsResetContext context)
        => _indexManager.SetLastTaskIdAsync(context.Settings.IndexName, 0);

    public override Task ValidatingAsync(ElasticsearchIndexSettingsValidatingContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Settings.IndexName))
        {
            context.Result.Fail(new ValidationResult(S["The index name is required."]));
        }
        else if (ElasticsearchIndexManager.ToSafeIndexName(context.Settings.IndexName) != context.Settings.IndexName)
        {
            context.Result.Fail(new ValidationResult(S["The index name contains forbidden characters."]));
        }

        return Task.CompletedTask;
    }
}
