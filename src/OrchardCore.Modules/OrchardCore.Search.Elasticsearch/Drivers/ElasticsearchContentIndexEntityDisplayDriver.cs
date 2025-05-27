using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.AzureAI.ViewModels;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Drivers;

internal sealed class ElasticsearchContentIndexEntityDisplayDriver : DisplayDriver<IndexEntity>
{
    internal readonly IStringLocalizer S;

    public ElasticsearchContentIndexEntityDisplayDriver(IStringLocalizer<ElasticsearchContentIndexEntityDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(IndexEntity index, BuildEditorContext context)
    {
        if (index.ProviderName != ElasticsearchConstants.ProviderName || index.Type != IndexingConstants.ContentsIndexSource)
        {
            return null;
        }

        return Initialize<ElasticsearchContentIndexEntityViewModel>("ElasticsearchContentIndexEntity_Edit", model =>
        {
            var metadata = index.As<ElasticsearchContentIndexMetadata>();

            model.StoreSourceData = metadata.StoreSourceData;
        }).Location("Content:6");
    }

    public override async Task<IDisplayResult> UpdateAsync(IndexEntity index, UpdateEditorContext context)
    {
        if (index.ProviderName != ElasticsearchConstants.ProviderName || index.Type != IndexingConstants.ContentsIndexSource)
        {
            return null;
        }

        var contentModel = new ElasticsearchContentIndexEntityViewModel();

        await context.Updater.TryUpdateModelAsync(contentModel, Prefix);

        var contentMetadata = index.As<ElasticsearchContentIndexMetadata>();

        contentMetadata.StoreSourceData = contentModel.StoreSourceData;

        index.Put(contentMetadata);

        return Edit(index, context);
    }
}
