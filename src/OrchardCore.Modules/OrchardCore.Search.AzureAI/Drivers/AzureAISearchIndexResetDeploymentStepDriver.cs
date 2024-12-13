using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.AzureAI.Deployment;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Search.AzureAI.Drivers;

public sealed class AzureAISearchIndexResetDeploymentStepDriver(AzureAISearchIndexSettingsService indexSettingsService)
    : DisplayDriver<DeploymentStep, AzureAISearchIndexResetDeploymentStep>
{
    private readonly AzureAISearchIndexSettingsService _indexSettingsService = indexSettingsService;

    public override Task<IDisplayResult> DisplayAsync(AzureAISearchIndexResetDeploymentStep step, BuildDisplayContext context)
        => CombineAsync(
            View("AzureAISearchIndexResetDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
            View("AzureAISearchIndexResetDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
        );

    public override IDisplayResult Edit(AzureAISearchIndexResetDeploymentStep step, BuildEditorContext context)
        => Initialize<AzureAISearchIndexResetDeploymentStepViewModel>("AzureAISearchIndexResetDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indices;
            model.AllIndexNames = (await _indexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");

    public override async Task<IDisplayResult> UpdateAsync(AzureAISearchIndexResetDeploymentStep step, UpdateEditorContext context)
    {
        step.Indices = [];

        await context.Updater.TryUpdateModelAsync(step, Prefix,
            p => p.IncludeAll,
            p => p.Indices);

        if (step.IncludeAll)
        {
            // Clear index names if the user select include all.
            step.Indices = [];
        }

        return Edit(step, context);
    }
}
