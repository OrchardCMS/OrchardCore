using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Search.AzureAI.Drivers;

internal sealed class AzureAISearchIndexEntityDisplayDriver : DisplayDriver<IndexEntity>
{
    private readonly AzureAISearchDefaultOptions _azureAIOptions;

    public AzureAISearchIndexEntityDisplayDriver(IOptions<AzureAISearchDefaultOptions> azureAIOptions)
    {
        _azureAIOptions = azureAIOptions.Value;
    }

    public override IDisplayResult Edit(IndexEntity index, BuildEditorContext context)
    {
        return Initialize<AzureAISettingsIndexEntityViewModel>("AzureAISearchIndexEntity_Edit", model =>
        {
            var metadata = index.As<AzureAISearchIndexMetadata>();

            model.AnalyzerName = metadata.AnalyzerName ?? AzureAISearchDefaultOptions.DefaultAnalyzer;
            model.QueryAnalyzerName = metadata.QueryAnalyzerName ?? AzureAISearchDefaultOptions.DefaultAnalyzer;
            model.Analyzers = _azureAIOptions.Analyzers.Select(x => new SelectListItem(x, x));
        }).Location("Content:1");
    }

    public override async Task<IDisplayResult> UpdateAsync(IndexEntity index, UpdateEditorContext context)
    {
        var model = new AzureAISettingsIndexEntityViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var metadata = index.As<AzureAISearchIndexMetadata>();

        metadata.AnalyzerName = model.AnalyzerName;
        metadata.QueryAnalyzerName = model.AnalyzerName;

        if (string.IsNullOrEmpty(metadata.AnalyzerName))
        {
            metadata.AnalyzerName = AzureAISearchDefaultOptions.DefaultAnalyzer;
        }

        if (string.IsNullOrEmpty(metadata.QueryAnalyzerName))
        {
            metadata.QueryAnalyzerName = metadata.AnalyzerName;
        }

        index.Put(metadata);

        return Edit(index, context);
    }
}
