using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
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
    private static readonly JsonWriterOptions _writerOptions = new JsonWriterOptions
    {
        SkipValidation = true,
    };

    private readonly ElasticsearchClient _elasticsearchClient;
    internal readonly IStringLocalizer S;

    public ElasticsearchIndexEntityHandler(
        ElasticsearchClient elasticsearchClient,
        IStringLocalizer<ElasticsearchIndexEntityHandler> stringLocalizer)
    {
        _elasticsearchClient = elasticsearchClient;
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

            if (string.IsNullOrEmpty(metadata.IndexMappings.KeyFieldName))
            {
                context.Result.Fail(new ValidationResult(S["The '{0}' is required.", nameof(ElasticsearchIndexMap.KeyFieldName)]));
            }
        }

        return Task.CompletedTask;
    }

    public override Task InitializingAsync(InitializingContext<IndexEntity> context)
       => PopulateAsync(context.Model, context.Data);

    private Task PopulateAsync(IndexEntity index, JsonNode data)
    {
        if (string.Equals(ElasticsearchConstants.ProviderName, index.ProviderName, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var metadata = index.As<ElasticsearchIndexMetadata>();

        metadata.IndexMappings ??= new ElasticsearchIndexMap();

        var indexMappings = data[nameof(metadata.IndexMappings)];

        if (indexMappings is not null)
        {
            using var mappingStream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(mappingStream, _writerOptions))
            {
                indexMappings.WriteTo(writer);
            }
            mappingStream.Position = 0;

            metadata.IndexMappings.Mapping = _elasticsearchClient.RequestResponseSerializer.Deserialize<TypeMapping>(mappingStream);
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
