using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing;
using OrchardCore.AzureAI.Deployment;
using OrchardCore.AzureAI.ViewModels;

namespace OrchardCore.AzureAI.Drivers;

public sealed class AzureAISearchIndexResetDeploymentStepDriver
    : DisplayDriver<DeploymentStep, AzureAISearchIndexResetDeploymentStep>
{
    private readonly IIndexProfileStore _store;

    internal readonly IStringLocalizer S;

    public AzureAISearchIndexResetDeploymentStepDriver(IIndexProfileStore store, IStringLocalizer<AzureAISearchIndexResetDeploymentStepDriver> stringLocalizer)
    {
        _store = store;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(AzureAISearchIndexResetDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("AzureAISearchIndexResetDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("AzureAISearchIndexResetDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(AzureAISearchIndexResetDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<AzureAISearchIndexResetDeploymentStepViewModel>("AzureAISearchIndexResetDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indices;
            model.AllIndexNames = (await _store.GetByProviderAsync(AzureAISearchConstants.ProviderName)).Select(x => x.IndexName).ToArray();
        }).Location(l => l.Zone("Content").Card(S["Indexes"], "10"));
    }

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
