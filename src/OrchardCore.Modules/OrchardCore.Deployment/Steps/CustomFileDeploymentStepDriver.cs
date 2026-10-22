using Microsoft.Extensions.Localization;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Deployment.Steps;

public sealed class CustomFileDeploymentStepDriver : DisplayDriver<DeploymentStep, CustomFileDeploymentStep>
{
    private readonly IStringLocalizer<CustomFileDeploymentStepDriver> S;

    public CustomFileDeploymentStepDriver(IStringLocalizer<CustomFileDeploymentStepDriver> stringLocalizer) =>
        S = stringLocalizer;

    public override Task<IDisplayResult> DisplayAsync(CustomFileDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("CustomFileDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("CustomFileDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(CustomFileDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<CustomFileDeploymentStepViewModel>("CustomFileDeploymentStep_Fields_Edit", model =>
        {
            model.FileContent = step.FileContent;
            model.FileName = step.FileName;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(CustomFileDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.FileName, x => x.FileContent);

        if (!string.IsNullOrWhiteSpace(step.FileName))
        {
            if (step.FileName.StartsWith('/') || Path.IsPathFullyQualified(step.FileName))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(step.FileName), S["A virtual path is required."]);
            }
            else if (PathExtensions.IsOutsideOfBasePath(Path.GetTempPath(), step.FileName))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(step.FileName), S["Path must be relative to the base directory."]);
            }
        }

        return Edit(step, context);
    }
}
