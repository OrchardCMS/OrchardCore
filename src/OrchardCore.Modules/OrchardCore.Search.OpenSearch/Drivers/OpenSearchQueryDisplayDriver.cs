using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Queries;
using OrchardCore.Search.OpenSearch.Core.Services;
using OrchardCore.Search.OpenSearch.Models;
using OrchardCore.Search.OpenSearch.ViewModels;

namespace OrchardCore.Search.OpenSearch.Drivers;

public sealed class OpenSearchQueryDisplayDriver : DisplayDriver<Query>
{
    private readonly IIndexProfileStore _store;

    internal readonly IStringLocalizer S;

    public OpenSearchQueryDisplayDriver(
        IIndexProfileStore store,
        IStringLocalizer<OpenSearchQueryDisplayDriver> stringLocalizer)
    {
        _store = store;
        S = stringLocalizer;
    }

    public override IDisplayResult Display(Query query, BuildDisplayContext context)
    {
        if (query.Source != OpenSearchQuerySource.SourceName)
        {
            return null;
        }

        return Combine(
            Dynamic("OpenSearchQuery_SummaryAdmin", model => { model.Query = query; }).Location("Content:5"),
            Dynamic("OpenSearchQuery_Buttons_SummaryAdmin", model => { model.Query = query; }).Location("Actions:2")
        );
    }

    public override IDisplayResult Edit(Query query, BuildEditorContext context)
    {
        if (query.Source != OpenSearchConstants.ProviderName)
        {
            return null;
        }

        return Initialize<OpenSearchQueryViewModel>("OpenSearchQuery_Edit", async model =>
        {
            var metadata = query.As<OpenSearchQueryMetadata>();

            model.Query = metadata.Template;
            model.Index = metadata.Index;
            model.ReturnContentItems = query.ReturnContentItems;
            model.Indexes = (await _store.GetByProviderAsync(OpenSearchConstants.ProviderName)).Select(x => new SelectListItem(x.Name, x.Name)).ToArray();

            // Extract query from the query string if we come from the main query editor.
            if (string.IsNullOrEmpty(metadata.Template))
            {
                await context.Updater.TryUpdateModelAsync(model, string.Empty, m => m.Query);
            }
        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(Query query, UpdateEditorContext context)
    {
        if (query.Source != OpenSearchQuerySource.SourceName)
        {
            return null;
        }

        var viewModel = new OpenSearchQueryViewModel();
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
        query.Put(new OpenSearchQueryMetadata
        {
            Template = viewModel.Query,
            Index = viewModel.Index,
        });

        return Edit(query, context);
    }
}
