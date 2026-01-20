using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Handlers;

public sealed class AzureAISearchIndexProfileHandler : IndexProfileHandlerBase
{
    internal readonly IStringLocalizer S;

    public AzureAISearchIndexProfileHandler(IStringLocalizer<AzureAISearchIndexProfileHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task InitializingAsync(InitializingContext<IndexProfile> context)
       => PopulateAsync(context.Model, context.Data);

    public override Task UpdatingAsync(UpdatingContext<IndexProfile> context)
        => PopulateAsync(context.Model, context.Data);

    public override Task ValidatingAsync(ValidatingContext<IndexProfile> context)
    {
        if (!CanHandle(context.Model))
        {
            return Task.CompletedTask;
        }

        if (!AzureAISearchIndexNamingHelper.TryGetSafeIndexName(context.Model.IndexName, out var safeName) || context.Model.IndexName != safeName)
        {
            context.Result.Fail(new ValidationResult(S["Invalid index name. Must start with a letter and be 1–128 characters long. Only letters, numbers, and underscores are allowed. Names cannot begin with 'azureSearch'."]));
        }

        var metadata = context.Model.As<AzureAISearchIndexMetadata>();

        if (metadata.IndexMappings is null || metadata.IndexMappings.Count == 0)
        {
            context.Result.Fail(new ValidationResult(S["At least one mapping field is required."]));
        }

        return Task.CompletedTask;
    }

    private static Task PopulateAsync(IndexProfile index, JsonNode data)
    {
        if (!CanHandle(index))
        {
            return Task.CompletedTask;
        }

        var metadata = index.As<AzureAISearchIndexMetadata>();

        // For backward compatibility, we look for 'AnalyzerName' and 'QueryAnalyzerName' in the data.
        var analyzerName = data[nameof(metadata.AnalyzerName)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(analyzerName))
        {
            metadata.AnalyzerName = analyzerName;
        }

        var indexMappings = data[nameof(metadata.IndexMappings)]?.AsArray();

        if (indexMappings is not null)
        {
            foreach (var indexMapping in indexMappings)
            {
                metadata.IndexMappings.Add(indexMapping.GetValue<AzureAISearchIndexMap>());
            }
        }

        index.Put(metadata);

        var queryMetadata = index.As<AzureAISearchDefaultQueryMetadata>();

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

    private static bool CanHandle(IndexProfile index)
        => string.Equals(AzureAISearchConstants.ProviderName, index.ProviderName, StringComparison.OrdinalIgnoreCase);
}
