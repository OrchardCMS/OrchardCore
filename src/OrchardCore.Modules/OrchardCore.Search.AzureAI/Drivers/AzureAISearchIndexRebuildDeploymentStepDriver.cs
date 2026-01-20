using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing;
using OrchardCore.Search.AzureAI.Deployment;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Search.AzureAI.Drivers;

public sealed class AzureAISearchIndexRebuildDeploymentStepDriver
    : DisplayDriver<DeploymentStep, AzureAISearchIndexRebuildDeploymentStep>
{
    private readonly IIndexProfileStore _indexProfileStore;

    public AzureAISearchIndexRebuildDeploymentStepDriver(IIndexProfileStore indexProfileStore)
    {
        _indexProfileStore = indexProfileStore;
    }

    public override Task<IDisplayResult> DisplayAsync(AzureAISearchIndexRebuildDeploymentStep step, BuildDisplayContext context)
        => CombineAsync(
            View("AzureAISearchIndexRebuildDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("AzureAISearchIndexRebuildDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
        );

    public override IDisplayResult Edit(AzureAISearchIndexRebuildDeploymentStep step, BuildEditorContext context)
        => Initialize<AzureAISearchIndexRebuildDeploymentStepViewModel>("AzureAISearchIndexRebuildDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indices;
            model.AllIndexNames = (await _indexProfileStore.GetByProviderAsync(AzureAISearchConstants.ProviderName)).Select(x => x.IndexName).ToArray();
        }).Location("Content");

    public override async Task<IDisplayResult> UpdateAsync(AzureAISearchIndexRebuildDeploymentStep step, UpdateEditorContext context)
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
