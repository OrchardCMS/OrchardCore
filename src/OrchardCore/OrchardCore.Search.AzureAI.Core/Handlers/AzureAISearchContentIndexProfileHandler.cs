using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Handlers;

public sealed class AzureAISearchContentIndexProfileHandler : IndexProfileHandlerBase
{
    private readonly AzureAISearchContentFieldMapper _mapper;

    internal readonly IStringLocalizer S;

    public AzureAISearchContentIndexProfileHandler(
        AzureAISearchContentFieldMapper mapper,
        IStringLocalizer<AzureAISearchContentIndexProfileHandler> stringLocalizer)
    {
        _mapper = mapper;
        S = stringLocalizer;
    }

    public override Task InitializingAsync(InitializingContext<IndexProfile> context)
        => SetMappingAsync(context.Model);

    /// <summary>
    /// Override the creating to allow creating the field mapping after the user selected content types from the UI.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task CreatingAsync(CreatingContext<IndexProfile> context)
    {
        await SetMappingAsync(context.Model);

        var queryMetadata = context.Model.As<AzureAISearchDefaultQueryMetadata>();

        if (queryMetadata.DefaultSearchFields is null || queryMetadata.DefaultSearchFields.Length == 0)
        {
            queryMetadata.DefaultSearchFields = [AzureAISearchIndexManager.FullTextKey];
        }

        var azureMetadata = context.Model.As<AzureAISearchIndexMetadata>();

        if (string.IsNullOrEmpty(queryMetadata.QueryAnalyzerName))
        {
            queryMetadata.QueryAnalyzerName = azureMetadata.AnalyzerName;
        }

        context.Model.Put(queryMetadata);
    }

    public override Task UpdatingAsync(UpdatingContext<IndexProfile> context)
        => SetMappingAsync(context.Model);

    private async Task SetMappingAsync(IndexProfile index)
    {
        if (!CanHandle(index))
        {
            return;
        }

        var azureMetadata = index.As<AzureAISearchIndexMetadata>();

        // Clear the existing mapping to ensure that only fields from the current definitions are included.
        azureMetadata.IndexMappings.Clear();

        var metadata = index.As<ContentIndexMetadata>();

        await _mapper.MapAsync(azureMetadata.IndexMappings, index, metadata.IndexedContentTypes, rootFields: true);

        index.Put(azureMetadata);
    }

    private static bool CanHandle(IndexProfile index)
    {
        return string.Equals(AzureAISearchConstants.ProviderName, index.ProviderName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(IndexingConstants.ContentsIndexSource, index.Type, StringComparison.OrdinalIgnoreCase);
    }
}
