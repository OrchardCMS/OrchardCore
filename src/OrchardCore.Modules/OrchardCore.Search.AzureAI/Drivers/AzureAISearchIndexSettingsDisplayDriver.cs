using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Search.AzureAI.Drivers;

internal sealed class AzureAISearchIndexSettingsDisplayDriver : DisplayDriver<AzureAISearchIndexSettings>
{
    private readonly AzureAISearchIndexManager _indexManager;
    private readonly IStringLocalizer S;

    public AzureAISearchIndexSettingsDisplayDriver(
        AzureAISearchIndexManager indexManager,
        IStringLocalizer<AzureAISearchIndexSettingsDisplayDriver> stringLocalizer)
    {
        _indexManager = indexManager;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(AzureAISearchIndexSettings settings, BuildDisplayContext context)
    {
        return CombineAsync(
            View("AzureAISearchIndexSettings_Fields_SummaryAdmin", settings).Location("Content:1"),
            View("AzureAISearchIndexSettings_Buttons_SummaryAdmin", settings).Location("Actions:5"),
            View("AzureAISearchIndexSettings_DefaultTags_SummaryAdmin", settings).Location("Tags:5")
        );
    }

    public override IDisplayResult Edit(AzureAISearchIndexSettings settings, BuildEditorContext context)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return Initialize<AzureAISettingsViewModel>("AzureAISearchIndexSettingsFields_Edit", model =>
        {
            model.AnalyzerName = settings.AnalyzerName ?? AzureAISearchDefaultOptions.DefaultAnalyzer;
            model.IndexName = settings.IndexName;
            model.IsNew = context.IsNew;
        }).Location("Content:1");
    }

    public override async Task<IDisplayResult> UpdateAsync(AzureAISearchIndexSettings settings, UpdateEditorContext context)
    {
        if (!string.Equals(AzureAISearchConstants.ContentsIndexSource, settings.Source, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var model = new AzureAISettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (context.IsNew)
        {
            if (string.IsNullOrWhiteSpace(model.IndexName))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexName), S["The index name is required."]);
            }
            else if (!AzureAISearchIndexNamingHelper.TryGetSafeIndexName(model.IndexName, out var indexName) || indexName != model.IndexName)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.IndexName), S["The index name contains forbidden characters."]);
            }
            else if (await _indexManager.ExistsAsync(model.IndexName))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(AzureAISettingsViewModel.IndexName), S["An index named <em>{0}</em> already exist in Azure AI Search server.", model.IndexName]);
            }

            settings.IndexName = model.IndexName;
            settings.IndexFullName = _indexManager.GetFullIndexName(model.IndexName);
        }

        settings.AnalyzerName = model.AnalyzerName;
        settings.QueryAnalyzerName = model.AnalyzerName;

        if (string.IsNullOrEmpty(settings.AnalyzerName))
        {
            settings.AnalyzerName = AzureAISearchDefaultOptions.DefaultAnalyzer;
        }

        if (string.IsNullOrEmpty(settings.QueryAnalyzerName))
        {
            settings.QueryAnalyzerName = settings.AnalyzerName;
        }

        return Edit(settings, context);
    }
}
