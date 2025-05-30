using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Lucene;
using OrchardCore.Search.Lucene.Models;
using OrchardCore.Search.Lucene.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Drivers;

internal sealed class LuceneContentIndexEntityDisplayDriver : DisplayDriver<IndexEntity>
{
    internal readonly IStringLocalizer S;

    public LuceneContentIndexEntityDisplayDriver(IStringLocalizer<LuceneContentIndexEntityDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(IndexEntity index, BuildEditorContext context)
    {
        if (index.ProviderName != LuceneConstants.ProviderName || index.Type != IndexingConstants.ContentsIndexSource)
        {
            return null;
        }

        return Initialize<LuceneContentIndexEntityViewModel>("LuceneContentIndexEntity_Edit", model =>
        {
            var metadata = index.As<LuceneContentIndexMetadata>();

            model.StoreSourceData = metadata.StoreSourceData;
        }).Location("Content:6");
    }

    public override async Task<IDisplayResult> UpdateAsync(IndexEntity index, UpdateEditorContext context)
    {
        if (index.ProviderName != LuceneConstants.ProviderName || index.Type != IndexingConstants.ContentsIndexSource)
        {
            return null;
        }

        var contentModel = new LuceneContentIndexEntityViewModel();

        await context.Updater.TryUpdateModelAsync(contentModel, Prefix);

        var contentMetadata = index.As<LuceneContentIndexMetadata>();

        contentMetadata.StoreSourceData = contentModel.StoreSourceData;

        index.Put(contentMetadata);

        return Edit(index, context);
    }
}
