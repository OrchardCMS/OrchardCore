using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Search.Lucene.Models;

namespace OrchardCore.Search.Lucene.Core.Handlers;

public sealed class LuceneIndexProfileHandler : IndexProfileHandlerBase
{
    public override Task InitializingAsync(InitializingContext<IndexProfile> context)
    {
        if (!CanHandle(context.Model))
        {
            return Task.CompletedTask;
        }

        var LuceneMetadata = context.Model.As<LuceneIndexMetadata>();

        var analyzerName = context.Data[nameof(LuceneMetadata.AnalyzerName)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(analyzerName))
        {
            LuceneMetadata.AnalyzerName = analyzerName;
        }

        var storeSourceData = context.Data[nameof(LuceneMetadata.StoreSourceData)]?.GetValue<bool>();

        if (storeSourceData.HasValue)
        {
            LuceneMetadata.StoreSourceData = storeSourceData.Value;
        }

        context.Model.Put(LuceneMetadata);

        return Task.CompletedTask;
    }

    private static bool CanHandle(IndexProfile index)
    {
        return string.Equals(LuceneConstants.ProviderName, index.ProviderName, StringComparison.OrdinalIgnoreCase);
    }
}
