using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing;
using OrchardCore.AzureAI.Deployment;
using OrchardCore.AzureAI.ViewModels;

namespace OrchardCore.AzureAI.Drivers;

public sealed class AzureAISearchIndexDeploymentStepDriver : DisplayDriver<DeploymentStep, AzureAISearchIndexDeploymentStep>
{
    private readonly IIndexProfileStore _store;

    internal readonly IStringLocalizer S;

    public AzureAISearchIndexDeploymentStepDriver(IIndexProfileStore store, IStringLocalizer<AzureAISearchIndexDeploymentStepDriver> stringLocalizer)
    {
        _store = store;
        S = stringLocalizer;
    }
    public override Task<IDisplayResult> DisplayAsync(AzureAISearchIndexDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("AzureAISearchIndexDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("AzureAISearchIndexDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(AzureAISearchIndexDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<AzureAISearchIndexDeploymentStepViewModel>("AzureAISearchIndexDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.IndexNames;
            model.AllIndexNames = (await _store.GetByProviderAsync(AzureAISearchConstants.ProviderName)).Select(x => x.IndexName).ToArray();
        }).Location(l => l.Zone("Content").Card(S["Indexes"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(AzureAISearchIndexDeploymentStep step, UpdateEditorContext context)
    {
        step.IndexNames = [];

        await context.Updater.TryUpdateModelAsync(step, Prefix,
            p => p.IncludeAll,
            p => p.IndexNames);

        if (step.IncludeAll)
        {
            // Clear index names if the user select include all.
            step.IndexNames = [];
        }

        return Edit(step, context);
    }
}
