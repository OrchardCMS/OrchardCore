using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Handlers;

public sealed class ElasticsearchIndexEntityHandler : IndexEntityHandlerBase
{
    internal readonly IStringLocalizer S;

    public ElasticsearchIndexEntityHandler(
        IStringLocalizer<ElasticsearchIndexEntityHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task ValidatingAsync(ValidatingContext<IndexEntity> context)
    {
        if (string.Equals(ElasticsearchConstants.ProviderName, context.Model.ProviderName, StringComparison.OrdinalIgnoreCase))
        {
            // When the provider is AzureAI, "regardless of the type" we need to validate the index mappings.
            var metadata = context.Model.As<ElasticsearchIndexMetadata>();

            if (metadata.IndexMappings?.Mapping?.Properties is null || !metadata.IndexMappings.Mapping.Properties.Any())
            {
                context.Result.Fail(new ValidationResult(S["At least one mapping property is required."]));
            }

            if (string.IsNullOrEmpty(metadata.KeyFieldName))
            {
                context.Result.Fail(new ValidationResult(S["The '{0}' is required.", nameof(metadata.KeyFieldName)]));
            }
        }

        return Task.CompletedTask;
    }

    public override Task InitializingAsync(InitializingContext<IndexEntity> context)
       => PopulateAsync(context.Model, context.Data);

    private static Task PopulateAsync(IndexEntity index, JsonNode data)
    {
        if (string.Equals(ElasticsearchConstants.ProviderName, index.ProviderName, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var metadata = index.As<ElasticsearchIndexMetadata>();

        // For backward compatibility, we look for 'AnalyzerName' and 'QueryAnalyzerName' in the data.
        var keyFieldName = data[nameof(metadata.KeyFieldName)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(keyFieldName))
        {
            metadata.KeyFieldName = keyFieldName;
        }

        var indexMappings = data[nameof(metadata.IndexMappings)];

        if (indexMappings is not null)
        {
            metadata.IndexMappings = JsonSerializer.Deserialize<ElasticsearchIndexMap>(indexMappings);
        }

        index.Put(metadata);

        var queryMetadata = index.As<ElasticsearchDefaultQueryMetadata>();

        var queryAnalyzerName = data[nameof(queryMetadata.QueryAnalyzerName)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(queryAnalyzerName))
        {
            queryMetadata.QueryAnalyzerName = queryAnalyzerName;
        }

        var defaultFields = data[nameof(queryMetadata.DefaultSearchFields)]?.AsArray();

        if (defaultFields is not null && defaultFields.Count > 0)
        {
            var fields = new List<string>();

            foreach (var field in defaultFields)
            {
                fields.Add(field.GetValue<string>());
            }

            queryMetadata.DefaultSearchFields = fields.ToArray();
        }

        index.Put(queryMetadata);

        return Task.CompletedTask;
    }
}
