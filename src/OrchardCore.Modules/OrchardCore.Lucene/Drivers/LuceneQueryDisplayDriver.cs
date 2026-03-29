using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Queries;
using OrchardCore.Lucene.Models;
using OrchardCore.Lucene.ViewModels;

namespace OrchardCore.Lucene.Drivers;

public sealed class LuceneQueryDisplayDriver : DisplayDriver<Query>
{
    private readonly IIndexProfileStore _indexStore;

    internal readonly IStringLocalizer S;

    public LuceneQueryDisplayDriver(
        IStringLocalizer<LuceneQueryDisplayDriver> stringLocalizer,
        IIndexProfileStore indexStore)
    {
        _indexStore = indexStore;
        S = stringLocalizer;
    }

    public override IDisplayResult Display(Query query, BuildDisplayContext context)
    {
        if (query.Source != LuceneQuerySource.SourceName)
        {
            return null;
        }

        return Combine(
            Dynamic("LuceneQuery_SummaryAdmin", static (model, query) => { model.Query = query; }, query)
                .Location("Content:5"),
            Dynamic("LuceneQuery_Buttons_SummaryAdmin", static (model, query) => { model.Query = query; }, query)
                .Location("Actions:2")
        );
    }

    public override async Task<IDisplayResult> EditAsync(Query query, BuildEditorContext context)
    {
        if (query.Source != LuceneQuerySource.SourceName)
        {
            return null;
        }

        // Create model object here, to make sure that TryUpdateModelAsync work on a specific object type, not over a proxied one
        var viewModel = new LuceneQueryViewModel();
        if (query.TryGet<LuceneQueryMetadata>(out var metadata))
        {
            viewModel.Query = metadata.Template;
            viewModel.Index = metadata.Index;
        }

        // Extract query from the query string if we come from the main query editor.
        if (string.IsNullOrEmpty(viewModel.Query))
        {
            await context.Updater.TryUpdateModelAsync(viewModel, string.Empty, m => m.Query);
        }

        return Initialize<LuceneQueryViewModel>("LuceneQuery_Edit", async model =>
        {
<<<<<<< HEAD:src/OrchardCore.Modules/OrchardCore.Lucene/Drivers/LuceneQueryDisplayDriver.cs
            model.Query = viewModel.Query;
            model.Index = viewModel.Index;
            model.Indexes = (await _indexStore.GetByProviderAsync(LuceneConstants.ProviderName)).Select(x => new SelectListItem(x.Name, x.Name)).ToArray();
            model.ReturnContentItems = query.ReturnContentItems;
=======
            if (query.TryGet<LuceneQueryMetadata>(out var metadata))
            {
                model.Query = metadata.Template;
                model.Index = metadata.Index;
            }

            model.ReturnContentItems = query.ReturnContentItems;
            model.Indexes = (await _indexStore.GetByProviderAsync(LuceneConstants.ProviderName)).Select(x => new SelectListItem(x.Name, x.Name)).ToArray();

            // Extract query from the query string if we come from the main query editor.
            if (string.IsNullOrEmpty(model.Query))
            {
                await context.Updater.TryUpdateModelAsync(model, string.Empty, m => m.Query);
            }
>>>>>>> 21e864e1b1 (Reduce Allocation by using `.TryGet<>` method over `.As<>`  (#19072)):src/OrchardCore.Modules/OrchardCore.Search.Lucene/Drivers/LuceneQueryDisplayDriver.cs
        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(Query query, UpdateEditorContext context)
    {
        if (query.Source != LuceneQuerySource.SourceName)
        {
            return null;
        }

        var viewModel = new LuceneQueryViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix,
            m => m.Query,
            m => m.Index,
            m => m.ReturnContentItems);

        if (string.IsNullOrWhiteSpace(viewModel.Query))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Query), S["The query field is required"]);
        }

        if (string.IsNullOrWhiteSpace(viewModel.Index))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Index), S["The index field is required"]);
        }

        query.ReturnContentItems = viewModel.ReturnContentItems;
        query.Put(new LuceneQueryMetadata()
        {
            Template = viewModel.Query,
            Index = viewModel.Index,
        });

        return await EditAsync(query, context);
    }
}
