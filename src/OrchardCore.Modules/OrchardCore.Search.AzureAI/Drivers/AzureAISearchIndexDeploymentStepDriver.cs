using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.AzureAI.Deployment;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Search.AzureAI.Drivers;

public class AzureAISearchIndexDeploymentStepDriver : DisplayDriver<DeploymentStep, AzureAISearchIndexDeploymentStep>
{
    private readonly AzureAISearchIndexSettingsService _indexSettingsService;

    public AzureAISearchIndexDeploymentStepDriver(AzureAISearchIndexSettingsService indexSettingsService)
    {
        _indexSettingsService = indexSettingsService;
    }

    public override IDisplayResult Display(AzureAISearchIndexDeploymentStep step)
    {
        return
            Combine(
                View("AzureAISearchIndexDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("AzureAISearchIndexDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AzureAISearchIndexDeploymentStep step)
    {
        return Initialize<AzureAISearchIndexDeploymentStepViewModel>("AzureAISearchIndexDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.IndexNames;
            model.AllIndexNames = (await _indexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(AzureAISearchIndexDeploymentStep step, IUpdateModel updater)
    {
        await updater.TryUpdateModelAsync(step, Prefix);

        // Don't have the selected option if include all.
        if (step.IncludeAll)
        {
            step.IndexNames = [];
        }

        return Edit(step);
    }
}
