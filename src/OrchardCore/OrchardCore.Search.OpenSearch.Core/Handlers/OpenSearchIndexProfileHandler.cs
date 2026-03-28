using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OpenSearch.Client;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Search.OpenSearch.Core.Models;
using OrchardCore.Search.OpenSearch.Core.Services;
using OrchardCore.Search.OpenSearch.Models;

namespace OrchardCore.Search.OpenSearch.Core.Handlers;

public sealed class OpenSearchIndexProfileHandler : IndexProfileHandlerBase
{
    private static readonly JsonWriterOptions _writerOptions = new()
    {
        SkipValidation = true,
    };

    private readonly OpenSearchClient _client;

    internal readonly IStringLocalizer S;

    public OpenSearchIndexProfileHandler(
        OpenSearchClient client,
        IStringLocalizer<OpenSearchIndexProfileHandler> stringLocalizer)
    {
        _client = client;
        S = stringLocalizer;
    }

    public override Task ValidatingAsync(ValidatingContext<IndexProfile> context)
    {
        if (!string.Equals(OpenSearchConstants.ProviderName, context.Model.ProviderName, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var safeName = OpenSearchIndexNameProvider.ToSafeIndexName(context.Model.IndexName);

        if (safeName != context.Model.IndexName)
        {
            context.Result.Fail(new ValidationResult(S["Invalid index name: it must be lowercase, under 255 bytes, not start with -, _, or +, and must not contain , /, *, ?, \", <, >, |, space, ,, or #, nor be \".\" or \"..\"."]));
        }

        var metadata = context.Model.As<OpenSearchIndexMetadata>();

        if (metadata.IndexMappings?.Mapping?.Properties is null || !metadata.IndexMappings.Mapping.Properties.Any())
        {
            context.Result.Fail(new ValidationResult(S["At least one mapping property is required."]));
        }

        if (string.IsNullOrEmpty(metadata.IndexMappings?.KeyFieldName))
        {
            context.Result.Fail(new ValidationResult(S["The '{0}' is required.", nameof(OpenSearchIndexMap.KeyFieldName)]));
        }

        return Task.CompletedTask;
    }

    public override Task InitializingAsync(InitializingContext<IndexProfile> context)
       => PopulateAsync(context.Model, context.Data);

    private Task PopulateAsync(IndexProfile index, JsonNode data)
    {
        if (!string.Equals(OpenSearchConstants.ProviderName, index.ProviderName, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var metadata = index.As<OpenSearchIndexMetadata>();

        metadata.IndexMappings ??= new OpenSearchIndexMap();

        var indexMappings = data[nameof(metadata.IndexMappings)];

        if (indexMappings is not null)
        {
            using var mappingStream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(mappingStream, _writerOptions))
            {
                indexMappings.WriteTo(writer);
            }
            mappingStream.Position = 0;

            metadata.IndexMappings.Mapping = _client.RequestResponseSerializer.Deserialize<TypeMapping>(mappingStream);
        }

        var analyzerName = data[nameof(metadata.AnalyzerName)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(analyzerName))
        {
            metadata.AnalyzerName = analyzerName;
        }

        var storeSourceData = data[nameof(metadata.StoreSourceData)]?.GetValue<bool>();

        if (storeSourceData.HasValue)
        {
            metadata.StoreSourceData = storeSourceData.Value;
        }

        var indexMapping = data[nameof(metadata.IndexMappings)]?.AsObject();

        if (indexMapping is not null && indexMapping.Count > 0)
        {
            metadata.IndexMappings = indexMapping.ToObject<OpenSearchIndexMap>();
        }

        index.Put(metadata);

        var queryMetadata = index.As<OpenSearchDefaultQueryMetadata>();

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
