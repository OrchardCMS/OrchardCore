using Lucene.Net.Util;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Contents.Indexing;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Lucene.Models;
using OrchardCore.Search.Lucene.Services;
using OrchardCore.Search.Lucene.ViewModels;

namespace OrchardCore.Search.Lucene.Drivers;

internal sealed class LuceneIndexProfileDisplayDriver : DisplayDriver<IndexProfile>
{
    private readonly LuceneAnalyzerManager _luceneAnalyzerManager;

    internal readonly IStringLocalizer S;

    public LuceneIndexProfileDisplayDriver(
        LuceneAnalyzerManager luceneAnalyzerManager,
        IStringLocalizer<LuceneIndexProfileDisplayDriver> stringLocalizer)
    {
        _luceneAnalyzerManager = luceneAnalyzerManager;
        S = stringLocalizer;
    }

    public override IDisplayResult Display(IndexProfile index, BuildDisplayContext context)
    {
        if (index.ProviderName != LuceneConstants.ProviderName)
        {
            return null;
        }

        return View("LuceneIndexProfile_ActionsMenuItems_SummaryAdmin", index)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:4");
    }

    public override IDisplayResult Edit(IndexProfile index, BuildEditorContext context)
    {
        if (index.ProviderName != LuceneConstants.ProviderName)
        {
            return null;
        }

        var data = Initialize<LuceneIndexProfileViewModel>("LuceneIndexProfile_Edit", model =>
        {
            var metadata = index.As<LuceneIndexMetadata>();

            model.StoreSourceData = metadata.StoreSourceData;
            model.AnalyzerName = metadata.AnalyzerName ?? LuceneConstants.DefaultAnalyzer;
            model.Analyzers = _luceneAnalyzerManager.GetAnalyzers().Select(x => new SelectListItem(x.Name, x.Name));
        }).Location("Content:5");

        var queryData = Initialize<LuceneDefaultQueryViewModel>("LuceneQuerySettings_Edit", model =>
        {
            var metadata = index.As<LuceneIndexMetadata>();
            var queryMetadata = index.As<LuceneIndexDefaultQueryMetadata>();

            model.QueryAnalyzerName = queryMetadata.QueryAnalyzerName ?? LuceneConstants.DefaultAnalyzer;
            model.Analyzers = _luceneAnalyzerManager.GetAnalyzers().Select(x => new SelectListItem(x.Name, x.Name));
            model.AllowLuceneQueries = queryMetadata.AllowLuceneQueries;
            model.DefaultVersions = new SelectListItem[]
            {
                new SelectListItem(S["Lucene 4.8"], nameof(LuceneVersion.LUCENE_48), queryMetadata.DefaultVersion == LuceneVersion.LUCENE_48),
            };

            if (metadata.IndexMappings?.Fields is null || metadata.IndexMappings.Fields.Length == 0)
            {
                model.DefaultSearchFields = [];
            }
            else
            {
                model.DefaultSearchFields = metadata.IndexMappings.Fields
                    .Select(field => new SelectListItem
                    {
                        Text = field,
                        Value = field,
                        Selected = queryMetadata.DefaultSearchFields?.Contains(field) ?? false ||
                        (context.IsNew && field == ContentIndexingConstants.FullTextKey),
                    }).OrderBy(x => x.Text)
                    .ToArray();
            }
        }).Location("Content:10");

        return Combine(data, queryData);
    }

    public override async Task<IDisplayResult> UpdateAsync(IndexProfile index, UpdateEditorContext context)
    {
        if (index.ProviderName != LuceneConstants.ProviderName)
        {
            return null;
        }

        var model = new LuceneIndexProfileViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var metadata = index.As<LuceneIndexMetadata>();

        metadata.StoreSourceData = model.StoreSourceData;
        metadata.AnalyzerName = model.AnalyzerName;

        if (string.IsNullOrEmpty(metadata.AnalyzerName))
        {
            metadata.AnalyzerName = LuceneConstants.DefaultAnalyzer;
        }

        index.Put(metadata);

        var queryModel = new LuceneDefaultQueryViewModel();

        await context.Updater.TryUpdateModelAsync(queryModel, Prefix);

        if (queryModel.DefaultSearchFields?.Length > 0)
        {
            index.Put(new LuceneIndexDefaultQueryMetadata
            {
                DefaultVersion = queryModel.DefaultVersion,
                QueryAnalyzerName = queryModel.QueryAnalyzerName ?? LuceneConstants.DefaultAnalyzer,
                AllowLuceneQueries = queryModel.AllowLuceneQueries,
                DefaultSearchFields = queryModel.DefaultSearchFields.Where(x => x.Selected).Select(x => x.Value).ToArray(),
            });
        }

        return Edit(index, context);
    }
}
