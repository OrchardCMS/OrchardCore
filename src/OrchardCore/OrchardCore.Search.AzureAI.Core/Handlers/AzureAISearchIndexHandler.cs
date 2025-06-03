using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Handlers;

public sealed class AzureAISearchIndexHandler : IndexEntityHandlerBase
{

    private readonly IStringLocalizer S;

    public AzureAISearchIndexHandler(IStringLocalizer<AzureAISearchIndexHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task InitializingAsync(InitializingContext<IndexEntity> context)
        => PopulateAsync(context.Model, context.Data);

    private static Task PopulateAsync(IndexEntity index, JsonNode data)
    {
        var metadata = index.As<AzureAISearchIndexMetadata>();

        var analyzerName = data[nameof(metadata.AnalyzerName)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(analyzerName))
        {
            metadata.AnalyzerName = analyzerName;
        }

        var indexMappings = data[nameof(metadata.IndexMappings)]?.AsArray();

        if (indexMappings is not null && indexMappings.Count > 0)
        {
            metadata.IndexMappings.Clear();

            foreach (var indexMapping in indexMappings)
            {
                var map = indexMapping.ToObject<AzureAISearchIndexMap>();

                metadata.IndexMappings.Add(map);
            }
        }

        index.Put(metadata);

        return Task.CompletedTask;
    }

    public override Task ValidatingAsync(ValidatingContext<IndexEntity> context)
    {
        if (!AzureAISearchIndexNamingHelper.TryGetSafeIndexName(context.Model.IndexName, out var indexName) || indexName != context.Model.IndexName)
        {
            context.Result.Fail(new ValidationResult(S["The index name contains forbidden characters."]));
        }

        return Task.CompletedTask;
    }
}
