using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Drivers;

internal sealed class ElasticIndexSettingsDisplayDriver : DisplayDriver<ElasticIndexSettings>
{
    private readonly ElasticsearchIndexManager _indexManager;
    private readonly ElasticsearchOptions _elasticsearchOptions;
    private readonly IStringLocalizer S;

    public ElasticIndexSettingsDisplayDriver(
        ElasticsearchIndexManager indexManager,
        IOptions<ElasticsearchOptions> elasticsearchOptions,
        IStringLocalizer<ElasticIndexSettingsDisplayDriver> stringLocalizer)
    {
        _indexManager = indexManager;
        _elasticsearchOptions = elasticsearchOptions.Value;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(ElasticIndexSettings settings, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ElasticIndexSettings_Fields_SummaryAdmin", settings)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),

            View("ElasticIndexSettings_Buttons_SummaryAdmin", settings)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5"),

            View("ElasticIndexSettings_ActionsMenuItems_SummaryAdmin", settings)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5"),

            View("ElasticIndexSettings_DefaultTags_SummaryAdmin", settings)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
        );
    }

    public override IDisplayResult Edit(ElasticIndexSettings settings, BuildEditorContext context)
    {
        return Initialize<ElasticIndexSettingsViewModel>("ElasticIndexSettingsFields_Edit", model =>
        {
            model.AnalyzerName = context.IsNew ? "standardanalyzer" : settings.AnalyzerName;
            model.IndexName = settings.IndexName;
            model.IsNew = context.IsNew;
            model.Analyzers = _elasticsearchOptions.Analyzers.Select(x => new SelectListItem(x.Key, x.Key));
        }).Location("Content:1");
    }

    public override async Task<IDisplayResult> UpdateAsync(ElasticIndexSettings settings, UpdateEditorContext context)
    {
        var model = new ElasticIndexSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (context.IsNew)
        {
            if (string.IsNullOrWhiteSpace(model.IndexName))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexName), S["The index name is required."]);
            }
            else if (ElasticsearchIndexNameService.ToSafeIndexName(model.IndexName) != model.IndexName)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexName), S["The index name contains forbidden characters."]);
            }
            else if (await _indexManager.ExistsAsync(model.IndexName))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexName), S["An index named <em>{0}</em> already exist in Elasticsearch server.", model.IndexName]);
            }

            settings.IndexName = model.IndexName;
        }

        settings.AnalyzerName = model.AnalyzerName;
        settings.QueryAnalyzerName = model.AnalyzerName;

        if (string.IsNullOrEmpty(settings.AnalyzerName))
        {
            settings.AnalyzerName = "standardanalyzer";
        }

        if (string.IsNullOrEmpty(settings.QueryAnalyzerName))
        {
            settings.QueryAnalyzerName = settings.AnalyzerName;
        }

        return Edit(settings, context);
    }
}
