using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Queries;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Models;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Drivers;

public sealed class ElasticQueryDisplayDriver : DisplayDriver<Query>
{
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

    internal readonly IStringLocalizer S;

    public ElasticQueryDisplayDriver(
        IStringLocalizer<ElasticQueryDisplayDriver> stringLocalizer,
        ElasticIndexSettingsService elasticIndexSettingsService)
    {
        _elasticIndexSettingsService = elasticIndexSettingsService;
        S = stringLocalizer;
    }

    public override IDisplayResult Display(Query query, BuildDisplayContext context)
    {
        if (query.Source != ElasticQuerySource.SourceName)
        {
            return null;
        }

        return Combine(
            Dynamic("ElasticQuery_SummaryAdmin", model => { model.Query = query; }).Location("Content:5"),
            Dynamic("ElasticQuery_Buttons_SummaryAdmin", model => { model.Query = query; }).Location("Actions:2")
        );
    }

    public override IDisplayResult Edit(Query query, BuildEditorContext context)
    {
        if (query.Source != ElasticQuerySource.SourceName)
        {
            return null;
        }

        return Initialize<ElasticQueryViewModel>("ElasticQuery_Edit", async model =>
        {
            var metadata = query.As<ElasticsearchQueryMetadata>();

            model.Query = metadata.Template;
            model.Index = metadata.Index;
            model.ReturnContentItems = query.ReturnContentItems;
            model.Indices = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();

            // Extract query from the query string if we come from the main query editor.
            if (string.IsNullOrEmpty(metadata.Template))
            {
                await context.Updater.TryUpdateModelAsync(model, string.Empty, m => m.Query);
            }
        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(Query query, UpdateEditorContext context)
    {
        if (query.Source != ElasticQuerySource.SourceName)
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

        return Edit(query, context);
    }
}
