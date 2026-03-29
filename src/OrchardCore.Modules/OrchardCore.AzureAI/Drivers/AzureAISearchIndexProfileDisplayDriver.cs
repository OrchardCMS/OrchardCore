using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Indexing.Models;
using OrchardCore.AzureAI.Models;
using OrchardCore.AzureAI.ViewModels;

namespace OrchardCore.AzureAI.Drivers;

internal sealed class AzureAISearchIndexProfileDisplayDriver : DisplayDriver<IndexProfile>
{
    private readonly IOptionsMonitor<AzureAISearchDefaultOptions> _azureAIOptions;

    public AzureAISearchIndexProfileDisplayDriver(IOptionsMonitor<AzureAISearchDefaultOptions> azureAIOptions)
    {
        _azureAIOptions = azureAIOptions;
    }

    public override IDisplayResult Edit(IndexProfile indexProfile, BuildEditorContext context)
    {
        if (indexProfile.ProviderName != AzureAISearchConstants.ProviderName)
        {
            return null;
        }

        var azureAIOptions = _azureAIOptions.CurrentValue;

        var data = Initialize<AzureAISettingsIndexProfileViewModel>("AzureAISearchIndexProfile_Edit", model =>
        {
            model.AnalyzerName = AzureAISearchDefaultOptions.DefaultAnalyzer;
<<<<<<< HEAD:src/OrchardCore.Modules/OrchardCore.AzureAI/Drivers/AzureAISearchIndexProfileDisplayDriver.cs

            if (indexProfile.TryGet<AzureAISearchIndexMetadata>(out var metadata))
            {
                model.AnalyzerName = metadata.AnalyzerName ?? AzureAISearchDefaultOptions.DefaultAnalyzer;
            }

            model.Analyzers = azureAIOptions.Analyzers.Select(x => new SelectListItem(x, x));
=======

            if (indexProfile.TryGet<AzureAISearchIndexMetadata>(out var metadata))
            {
                model.AnalyzerName = metadata.AnalyzerName ?? AzureAISearchDefaultOptions.DefaultAnalyzer;
            }

            model.Analyzers = _azureAIOptions.Analyzers.Select(x => new SelectListItem(x, x));
>>>>>>> 21e864e1b1 (Reduce Allocation by using `.TryGet<>` method over `.As<>`  (#19072)):src/OrchardCore.Modules/OrchardCore.Search.AzureAI/Drivers/AzureAISearchIndexProfileDisplayDriver.cs
        }).Location("Content:5");

        var queryData = Initialize<AzureAISearchDefaultQueryViewModel>("AzureAISearchQuerySettings_Edit", model =>
        {
            model.QueryAnalyzerName = AzureAISearchDefaultOptions.DefaultAnalyzer;
<<<<<<< HEAD:src/OrchardCore.Modules/OrchardCore.AzureAI/Drivers/AzureAISearchIndexProfileDisplayDriver.cs
            model.Analyzers = azureAIOptions.Analyzers.Select(x => new SelectListItem(x, x));

            string[] defaultSearchFields = null;

            if (indexProfile.TryGet<AzureAISearchDefaultQueryMetadata>(out var metadata))
            {
                model.QueryAnalyzerName = metadata.QueryAnalyzerName ?? AzureAISearchDefaultOptions.DefaultAnalyzer;
                defaultSearchFields = metadata.DefaultSearchFields;
            }

=======
            model.Analyzers = _azureAIOptions.Analyzers.Select(x => new SelectListItem(x, x));

            string[] defaultSearchFields = null;

            if (indexProfile.TryGet<AzureAISearchDefaultQueryMetadata>(out var metadata))
            {
                model.QueryAnalyzerName = metadata.QueryAnalyzerName ?? AzureAISearchDefaultOptions.DefaultAnalyzer;
                defaultSearchFields = metadata.DefaultSearchFields;
            }

>>>>>>> 21e864e1b1 (Reduce Allocation by using `.TryGet<>` method over `.As<>`  (#19072)):src/OrchardCore.Modules/OrchardCore.Search.AzureAI/Drivers/AzureAISearchIndexProfileDisplayDriver.cs
            if (indexProfile.TryGet<AzureAISearchIndexMetadata>(out var indexMetadata) && indexMetadata.IndexMappings?.Count > 0)
            {
                model.DefaultSearchFields = indexMetadata.IndexMappings
                .Where(x => x.IsSearchable)
                .Select(x => new SelectListItem
                {
                    Text = x.AzureFieldKey,
                    Value = x.AzureFieldKey,
                    Selected = defaultSearchFields?.Contains(x.AzureFieldKey) ?? false,
                }).OrderBy(x => x.Text)
                .ToArray();
            }
        }).Location("Content:10");

        return Combine(data, queryData);
    }

    public override async Task<IDisplayResult> UpdateAsync(IndexProfile indexProfile, UpdateEditorContext context)
    {
        if (indexProfile.ProviderName != AzureAISearchConstants.ProviderName)
        {
            return null;
        }

        var model = new AzureAISettingsIndexProfileViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var metadata = indexProfile.GetOrCreate<AzureAISearchIndexMetadata>();

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
