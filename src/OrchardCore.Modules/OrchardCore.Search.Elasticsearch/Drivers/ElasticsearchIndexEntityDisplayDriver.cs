using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Contents.Indexing;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.AzureAI.ViewModels;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Drivers;

internal sealed class ElasticsearchIndexEntityDisplayDriver : DisplayDriver<IndexEntity>
{
    private readonly ElasticsearchOptions _elasticsearchOptions;

    internal readonly IStringLocalizer S;

    public ElasticsearchIndexEntityDisplayDriver(
        IOptions<ElasticsearchOptions> elasticsearchOptions,
        IStringLocalizer<ElasticsearchIndexEntityDisplayDriver> stringLocalizer)
    {
        _elasticsearchOptions = elasticsearchOptions.Value;
        S = stringLocalizer;
    }

    public override IDisplayResult Display(IndexEntity index, BuildDisplayContext context)
    {
        if (index.ProviderName != ElasticsearchConstants.ProviderName)
        {
            return null;
        }

        return View("ElasticsearchIndexEntity_ActionsMenuItems_SummaryAdmin", index)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:4");
    }

    public override IDisplayResult Edit(IndexEntity index, BuildEditorContext context)
    {
        if (index.ProviderName != ElasticsearchConstants.ProviderName)
        {
            return null;
        }

        var data = Initialize<ElasticsearchIndexEntityViewModel>("ElasticsearchIndexEntity_Edit", model =>
        {
            var metadata = index.As<ElasticsearchIndexMetadata>();

            model.AnalyzerName = metadata.AnalyzerName ?? ElasticsearchConstants.DefaultAnalyzer;
            model.Analyzers = _elasticsearchOptions.Analyzers.Select(x => new SelectListItem(x.Key, x.Key));
        }).Location("Content:5");

        var queryData = Initialize<ElasticsearchDefaultQueryViewModel>("ElasticsearchQuerySettings_Edit", model =>
        {
            var metadata = index.As<ElasticsearchIndexMetadata>();
            var queryMetadata = index.As<ElasticsearchDefaultQueryMetadata>();

            model.QueryAnalyzerName = queryMetadata.QueryAnalyzerName ?? ElasticsearchConstants.DefaultAnalyzer;
            model.Analyzers = _elasticsearchOptions.Analyzers.Select(x => new SelectListItem(x.Key, x.Key));
            model.SearchTypes =
            [
                new(S["Multi-Match Query (Default)"], string.Empty),
                new(S["Query String Query"], ElasticsearchConstants.QueryStringSearchType),
                new(S["Custom Query"], ElasticsearchConstants.CustomSearchType),
            ];

            if (metadata.IndexMappings?.Mapping?.Properties is null || !metadata.IndexMappings.Mapping.Properties.Any())
            {
                model.DefaultSearchFields = [];
            }
            else
            {
                model.DefaultSearchFields = metadata.IndexMappings.GetFieldPaths()
                    .Select(propertyName => new SelectListItem
                    {
                        Text = propertyName,
                        Value = propertyName,
                        Selected = queryMetadata.DefaultSearchFields?.Contains(propertyName) ?? false ||
                        (context.IsNew && propertyName == ContentIndexingConstants.FullTextKey),
                    }).OrderBy(x => x.Text)
                    .ToArray();
            }
        }).Location("Content:10");

        return Combine(data, queryData);
    }

    public override async Task<IDisplayResult> UpdateAsync(IndexEntity index, UpdateEditorContext context)
    {
        if (index.ProviderName != ElasticsearchConstants.ProviderName)
        {
            return null;
        }

        var model = new ElasticsearchIndexEntityViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var metadata = index.As<ElasticsearchIndexMetadata>();

        metadata.AnalyzerName = model.AnalyzerName;

        if (string.IsNullOrEmpty(metadata.AnalyzerName))
        {
            metadata.AnalyzerName = ElasticsearchConstants.DefaultAnalyzer;
        }

        index.Put(metadata);

        var queryModel = new ElasticsearchDefaultQueryViewModel();

        await context.Updater.TryUpdateModelAsync(queryModel, Prefix);

        if (queryModel.DefaultSearchFields?.Length > 0)
        {
            index.Put(new ElasticsearchDefaultQueryMetadata
            {
                QueryAnalyzerName = queryModel.QueryAnalyzerName ?? ElasticsearchConstants.DefaultAnalyzer,
                DefaultQuery = queryModel.DefaultQuery,
                SearchType = queryModel.SearchType,
                DefaultSearchFields = queryModel.DefaultSearchFields.Where(x => x.Selected).Select(x => x.Value).ToArray(),
            });
        }

        return Edit(index, context);
    }
}
