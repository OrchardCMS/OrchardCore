using Microsoft.Extensions.Localization;
using OrchardCore.Deployment.Services;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Steps;

public sealed class CustomFileDeploymentStepDriver : DisplayDriver<DeploymentStep, CustomFileDeploymentStep>
{
    private readonly IStringLocalizer S;

    /// <summary>Creates the file editor with shared deployment validation.</summary>
    public CustomFileDeploymentStepDriver(IStringLocalizer<CustomFileDeploymentStepDriver> localizer) => S = localizer;

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
        var candidate = new CustomFileDeploymentStep { FileName = step.FileName, FileContent = step.FileContent };
        await context.Updater.TryUpdateModelAsync(candidate, Prefix, x => x.FileName, x => x.FileContent);
        var errors = DeploymentStepValidation.Validate(candidate);
        foreach (var messages in errors.Values)
        {
            foreach (var message in messages)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(CustomFileDeploymentStepViewModel.FileName), S[message]);
            }
        }
        if (errors.Count == 0)
        {
            DeploymentStepValidation.Normalize(candidate);
            step.FileName = candidate.FileName;
            step.FileContent = candidate.FileContent;
        }

        return Edit(step, context);
    }
}
