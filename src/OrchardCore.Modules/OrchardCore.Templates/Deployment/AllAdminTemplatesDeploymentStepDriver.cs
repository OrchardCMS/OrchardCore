using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Deployment;

public sealed class AllAdminTemplatesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllAdminTemplatesDeploymentStep>
{
    internal readonly IStringLocalizer S;

    public AllAdminTemplatesDeploymentStepDriver(IStringLocalizer<AllAdminTemplatesDeploymentStepDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(AllAdminTemplatesDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllAdminTemplatesDeploymentStep_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("AllAdminTemplatesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllAdminTemplatesDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<AllAdminTemplatesDeploymentStepViewModel>("AllAdminTemplatesDeploymentStep_Fields_Edit", model =>
        {
            model.ExportAsFiles = step.ExportAsFiles;
        }).Location(l => l.Zone("Content").Card(S["Admin templates"], "10"));
    }
    public override async Task<IDisplayResult> UpdateAsync(AllAdminTemplatesDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.ExportAsFiles);

        return Edit(step, context);
    }
}
