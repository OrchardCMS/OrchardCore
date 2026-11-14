using Microsoft.Extensions.Localization;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Drivers;

/// <summary>
/// Builds the editor of a deployment plan, on the pages creating and editing a plan, whose fields are grouped in cards
/// by their placement.
/// </summary>
internal sealed class DeploymentPlanDisplayDriver : DisplayDriver<CreateDeploymentPlanViewModel>
{
    internal readonly IStringLocalizer S;

    public DeploymentPlanDisplayDriver(IStringLocalizer<DeploymentPlanDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(CreateDeploymentPlanViewModel model, BuildEditorContext context)
    {
        return Initialize<CreateDeploymentPlanViewModel>("DeploymentPlanGeneral_Edit", m => m.Name = model.Name)
            .Location(l => l.Zone("Content", "1").Card(S["General"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(CreateDeploymentPlanViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Name);

        return Edit(model, context);
    }

    // The fields keep the names the pages post, which the controller validates.
    protected override void BuildPrefix(CreateDeploymentPlanViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
