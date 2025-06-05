using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Contents.Indexing;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Drivers;

internal sealed class ElasticsearchIndexProfileDisplayDriver : DisplayDriver<IndexProfile>
{
    private readonly ElasticsearchOptions _elasticsearchOptions;

    internal readonly IStringLocalizer S;

    public ElasticsearchIndexProfileDisplayDriver(
        IOptions<ElasticsearchOptions> elasticsearchOptions,
        IStringLocalizer<ElasticsearchIndexProfileDisplayDriver> stringLocalizer)
    {
        _elasticsearchOptions = elasticsearchOptions.Value;
        S = stringLocalizer;
    }

    public override IDisplayResult Display(IndexProfile index, BuildDisplayContext context)
    {
        if (index.ProviderName != ElasticsearchConstants.ProviderName)
        {
            return null;
        }

        return View("ElasticsearchIndexProfile_ActionsMenuItems_SummaryAdmin", index)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:4");
    }

    public override IDisplayResult Edit(IndexProfile indexProfile, BuildEditorContext context)
    {
        if (indexProfile.ProviderName != ElasticsearchConstants.ProviderName)
        {
            return null;
        }

        var data = Initialize<ElasticsearchIndexProfileViewModel>("ElasticsearchIndexProfile_Edit", model =>
        {
            var metadata = indexProfile.As<ElasticsearchIndexMetadata>();

            model.StoreSourceData = metadata.StoreSourceData;
            model.AnalyzerName = metadata.AnalyzerName ?? ElasticsearchConstants.DefaultAnalyzer;
            model.Analyzers = _elasticsearchOptions.Analyzers.Select(x => new SelectListItem(x.Key, x.Key));
        }).Location("Content:5");

        var queryData = Initialize<ElasticsearchDefaultQueryViewModel>("ElasticsearchQuerySettings_Edit", model =>
        {
            var metadata = indexProfile.As<ElasticsearchIndexMetadata>();
            var queryMetadata = indexProfile.As<ElasticsearchDefaultQueryMetadata>();

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

    public override async Task<IDisplayResult> UpdateAsync(IndexProfile indexProfile, UpdateEditorContext context)
    {
        if (indexProfile.ProviderName != ElasticsearchConstants.ProviderName)
        {
            return null;
        }

        var model = new ElasticsearchIndexProfileViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var metadata = indexProfile.As<ElasticsearchIndexMetadata>();

        metadata.StoreSourceData = model.StoreSourceData;
        metadata.AnalyzerName = model.AnalyzerName;

        if (string.IsNullOrEmpty(metadata.AnalyzerName))
        {
            metadata.AnalyzerName = ElasticsearchConstants.DefaultAnalyzer;
        }

        indexProfile.Put(metadata);

        var queryModel = new ElasticsearchDefaultQueryViewModel();

        await context.Updater.TryUpdateModelAsync(queryModel, Prefix);

        if (queryModel.DefaultSearchFields?.Length > 0)
        {
            indexProfile.Put(new ElasticsearchDefaultQueryMetadata
            {
                QueryAnalyzerName = queryModel.QueryAnalyzerName ?? ElasticsearchConstants.DefaultAnalyzer,
                DefaultQuery = queryModel.DefaultQuery,
                SearchType = queryModel.SearchType,
                DefaultSearchFields = queryModel.DefaultSearchFields.Where(x => x.Selected).Select(x => x.Value).ToArray(),
            });
        }

        return Edit(indexProfile, context);
    }
}
