using Microsoft.Extensions.Localization;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment;

public sealed class AllContentDeploymentStepDriver : DisplayDriver<DeploymentStep, AllContentDeploymentStep>
{
    internal readonly IStringLocalizer S;

    public AllContentDeploymentStepDriver(IStringLocalizer<AllContentDeploymentStepDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(AllContentDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllContentDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("AllContentDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllContentDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<AllContentDeploymentStepViewModel>("AllContentDeploymentStep_Fields_Edit", model =>
        {
            model.ExportAsSetupRecipe = step.ExportAsSetupRecipe;
        }).Location(l => l.Zone("Content").Card(S["Export"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(AllContentDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.ExportAsSetupRecipe);

        return Edit(step, context);
    }
}
