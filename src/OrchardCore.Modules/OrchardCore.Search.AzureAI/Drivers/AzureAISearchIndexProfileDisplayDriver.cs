using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Search.AzureAI.Drivers;

internal sealed class AzureAISearchIndexProfileDisplayDriver : DisplayDriver<IndexProfile>
{
    private readonly AzureAISearchDefaultOptions _azureAIOptions;

    public AzureAISearchIndexProfileDisplayDriver(IOptions<AzureAISearchDefaultOptions> azureAIOptions)
    {
        _azureAIOptions = azureAIOptions.Value;
    }

    public override IDisplayResult Edit(IndexProfile indexProfile, BuildEditorContext context)
    {
        if (indexProfile.ProviderName != AzureAISearchConstants.ProviderName || indexProfile.Type != IndexingConstants.ContentsIndexSource)
        {
            return null;
        }

        var data = Initialize<AzureAISettingsIndexProfileViewModel>("AzureAISearchIndexProfile_Edit", model =>
        {
            var metadata = indexProfile.As<AzureAISearchIndexMetadata>();

            model.AnalyzerName = metadata.AnalyzerName ?? AzureAISearchDefaultOptions.DefaultAnalyzer;
            model.Analyzers = _azureAIOptions.Analyzers.Select(x => new SelectListItem(x, x));
        }).Location("Content:5");

        var queryData = Initialize<AzureAISearchDefaultQueryViewModel>("AzureAISearchQuerySettings_Edit", model =>
        {
            var metadata = indexProfile.As<AzureAISearchDefaultQueryMetadata>();

            var indexMetadata = indexProfile.As<AzureAISearchIndexMetadata>();

            model.QueryAnalyzerName = metadata.QueryAnalyzerName ?? AzureAISearchDefaultOptions.DefaultAnalyzer;
            model.Analyzers = _azureAIOptions.Analyzers.Select(x => new SelectListItem(x, x));

            if (indexMetadata.IndexMappings?.Count > 0)
            {
                model.DefaultSearchFields = indexMetadata.IndexMappings
                .Where(x => x.IsSearchable)
                .Select(x => new SelectListItem
                {
                    Text = x.AzureFieldKey,
                    Value = x.AzureFieldKey,
                    Selected = metadata.DefaultSearchFields?.Contains(x.AzureFieldKey) ?? false,
                }).OrderBy(x => x.Text)
                .ToArray();
            }
        }).Location("Content:10");

        return Combine(data, queryData);
    }

    public override async Task<IDisplayResult> UpdateAsync(IndexProfile indexProfile, UpdateEditorContext context)
    {
        if (indexProfile.ProviderName != AzureAISearchConstants.ProviderName || indexProfile.Type != IndexingConstants.ContentsIndexSource)
        {
            return null;
        }

        var model = new AzureAISettingsIndexProfileViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var metadata = indexProfile.As<AzureAISearchIndexMetadata>();

        metadata.AnalyzerName = model.AnalyzerName;

        if (string.IsNullOrEmpty(metadata.AnalyzerName))
        {
            metadata.AnalyzerName = AzureAISearchDefaultOptions.DefaultAnalyzer;
        }

        indexProfile.Put(metadata);

        var queryModel = new AzureAISearchDefaultQueryViewModel();

        await context.Updater.TryUpdateModelAsync(queryModel, Prefix);

        if (queryModel.DefaultSearchFields?.Length > 0)
        {
            indexProfile.Put(new AzureAISearchDefaultQueryMetadata
            {
                QueryAnalyzerName = !string.IsNullOrEmpty(queryModel.QueryAnalyzerName)
                    ? queryModel.QueryAnalyzerName
                    : AzureAISearchDefaultOptions.DefaultAnalyzer,
                DefaultSearchFields = queryModel.DefaultSearchFields.Where(x => x.Selected).Select(x => x.Value).ToArray(),
            });
        }

        return Edit(indexProfile, context);
    }
}
