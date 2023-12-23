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

public class AzureAISearchIndexResetDeploymentStepDriver(AzureAISearchIndexSettingsService indexSettingsService)
    : DisplayDriver<DeploymentStep, AzureAISearchIndexResetDeploymentStep>
{
    private readonly AzureAISearchIndexSettingsService _indexSettingsService = indexSettingsService;


    public override IDisplayResult Display(AzureAISearchIndexResetDeploymentStep step)
    {
        return
            Combine(
                View("AzureAISearchIndexResetDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("AzureAISearchIndexResetDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AzureAISearchIndexResetDeploymentStep step)
    {
        return Initialize<AzureAISearchIndexResetDeploymentStepViewModel>("AzureAISearchIndexResetDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indices;
            model.AllIndexNames = (await _indexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(AzureAISearchIndexResetDeploymentStep resetIndexStep, IUpdateModel updater)
    {
        resetIndexStep.Indices = [];

        await updater.TryUpdateModelAsync(resetIndexStep, Prefix, step => step.Indices, step => step.IncludeAll);

        if (resetIndexStep.IncludeAll)
        {
            resetIndexStep.Indices = [];
        }

        return Edit(resetIndexStep);
    }
}
