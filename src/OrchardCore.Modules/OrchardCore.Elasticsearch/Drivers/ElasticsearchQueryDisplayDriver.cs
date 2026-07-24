using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Queries;
using OrchardCore.Elasticsearch.Core.Services;
using OrchardCore.Elasticsearch.Models;
using OrchardCore.Elasticsearch.ViewModels;

namespace OrchardCore.Elasticsearch.Drivers;

public sealed class ElasticsearchQueryDisplayDriver : DisplayDriver<Query>
{
    private readonly IIndexProfileStore _store;

    internal readonly IStringLocalizer S;

    public ElasticsearchQueryDisplayDriver(
        IIndexProfileStore store,
        IStringLocalizer<ElasticsearchQueryDisplayDriver> stringLocalizer)
    {
        _store = store;
        S = stringLocalizer;
    }

    public override IDisplayResult Display(Query query, BuildDisplayContext context)
    {
        if (query.Source != ElasticsearchQuerySource.SourceName)
        {
            return null;
        }

        return Combine(
            Dynamic("ElasticQuery_SummaryAdmin", static (model, query) => { model.Query = query; }, query).Location("Content:5"),
            Dynamic("ElasticQuery_Buttons_SummaryAdmin", static (model, query) => { model.Query = query; }, query).Location("Actions:2")
        );
    }

    public override async Task<IDisplayResult> EditAsync(Query query, BuildEditorContext context)
    {
        if (query.Source != ElasticsearchConstants.ProviderName)
        {
            return null;
        }

        // Create model object here, to make sure that TryUpdateModelAsync work on a specific object type, not over a proxied one
        var viewModel = new ElasticQueryViewModel();
        if (query.TryGet<ElasticsearchQueryMetadata>(out var metadata))
        {
            viewModel.Query = metadata.Template;
            viewModel.Index = metadata.Index;
        }

        // Extract query from the query string if we come from the main query editor.
        if (string.IsNullOrEmpty(viewModel.Query))
        {
            await context.Updater.TryUpdateModelAsync(viewModel, string.Empty, m => m.Query);
        }

        return Initialize<ElasticQueryViewModel>("ElasticQuery_Edit", async model =>
        {
            model.Query = viewModel.Query;
            model.Index = viewModel.Index;
            model.Indexes = (await _store.GetByProviderAsync(ElasticsearchConstants.ProviderName)).Select(x => new SelectListItem(x.Name, x.Name)).ToArray();
            model.ReturnContentItems = query.ReturnContentItems;
        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(Query query, UpdateEditorContext context)
    {
        if (query.Source != ElasticsearchQuerySource.SourceName)
        {
            return null;
        }

        var viewModel = new ElasticQueryViewModel();
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
        query.Put(new ElasticsearchQueryMetadata
        {
            Template = viewModel.Query,
            Index = viewModel.Index,
        });

        return await EditAsync(query, context);
    }
}
