using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OrchardCore.Contents.Indexing;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing.Models;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.OpenSearch.Core.Models;
using OrchardCore.OpenSearch.ViewModels;

namespace OrchardCore.OpenSearch.Drivers;

internal sealed class OpenSearchIndexProfileDisplayDriver : DisplayDriver<IndexProfile>
{
    private readonly OpenSearchOptions _openSearchOptions;
    private readonly OpenSearchClient _openSearchClient;

    internal readonly IStringLocalizer S;

    public OpenSearchIndexProfileDisplayDriver(
        OpenSearchClient openSearchClient,
        IOptions<OpenSearchOptions> openSearchOptions,
        IStringLocalizer<OpenSearchIndexProfileDisplayDriver> stringLocalizer)
    {
        _openSearchOptions = openSearchOptions.Value;
        _openSearchClient = openSearchClient;
        S = stringLocalizer;
    }

    public override IDisplayResult Display(IndexProfile index, BuildDisplayContext context)
    {
        if (index.ProviderName != OpenSearchConstants.ProviderName)
        {
            return null;
        }

        return View("OpenSearchIndexProfile_ActionsMenuItems_SummaryAdmin", index)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:4");
    }

    public override IDisplayResult Edit(IndexProfile indexProfile, BuildEditorContext context)
    {
        if (indexProfile.ProviderName != OpenSearchConstants.ProviderName)
        {
            return null;
        }

        var data = Initialize<OpenSearchIndexProfileViewModel>("OpenSearchIndexProfile_Edit", model =>
        {
            var metadata = indexProfile.As<OpenSearchIndexMetadata>();

            model.StoreSourceData = metadata.StoreSourceData;
            model.AnalyzerName = metadata.AnalyzerName ?? OpenSearchConstants.DefaultAnalyzer;
            model.Analyzers = _openSearchOptions.Analyzers.Select(x => new SelectListItem(x.Key, x.Key));
        }).Location("Content:5");

        var queryData = Initialize<OpenSearchDefaultQueryViewModel>("OpenSearchQuerySettings_Edit", model =>
        {
            var queryMetadata = indexProfile.As<OpenSearchDefaultQueryMetadata>();

            model.SearchType = queryMetadata.SearchType;
            model.DefaultQuery = queryMetadata.DefaultQuery;
            model.QueryAnalyzerName = queryMetadata.QueryAnalyzerName ?? OpenSearchConstants.DefaultAnalyzer;
            model.Analyzers = _openSearchOptions.Analyzers.Select(x => new SelectListItem(x.Key, x.Key));
            model.SearchTypes =
            [
                new(S["Multi-Match Query (Default)"], string.Empty),
                new(S["Query String Query"], OpenSearchConstants.QueryStringSearchType),
                new(S["Custom Query"], OpenSearchConstants.CustomSearchType),
            ];

            var metadata = indexProfile.As<OpenSearchIndexMetadata>();

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
        if (indexProfile.ProviderName != OpenSearchConstants.ProviderName)
        {
            return null;
        }

        var model = new OpenSearchIndexProfileViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var metadata = indexProfile.As<OpenSearchIndexMetadata>();

        metadata.StoreSourceData = model.StoreSourceData;
        metadata.AnalyzerName = model.AnalyzerName;

        if (string.IsNullOrEmpty(metadata.AnalyzerName))
        {
            metadata.AnalyzerName = OpenSearchConstants.DefaultAnalyzer;
        }

        indexProfile.Put(metadata);

        var queryModel = new OpenSearchDefaultQueryViewModel();

        await context.Updater.TryUpdateModelAsync(queryModel, Prefix);

        var queryMetadata = new OpenSearchDefaultQueryMetadata
        {
            QueryAnalyzerName = queryModel.QueryAnalyzerName ?? OpenSearchConstants.DefaultAnalyzer,
            DefaultQuery = queryModel.DefaultQuery,
            SearchType = queryModel.SearchType,
            DefaultSearchFields = queryModel.DefaultSearchFields?.Where(x => x.Selected)?.Select(x => x.Value).ToArray(),
        };

        if (queryModel.SearchType == OpenSearchConstants.CustomSearchType)
        {
            var query = queryModel.DefaultQuery?.Trim();

            if (string.IsNullOrEmpty(query))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(queryModel.DefaultQuery), S["The default query field is required when using a custom search type."]);
            }
            else
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(query));

                try
                {
                    _openSearchClient.RequestResponseSerializer.Deserialize<SearchRequest>(stream);

                    queryMetadata.DefaultQuery = query;
                }
                catch
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(queryModel.DefaultQuery), S["The default query field is not a valid OpenSearch query."]);
                }
            }
        }

        indexProfile.Put(queryMetadata);

        return Edit(indexProfile, context);
    }
}
