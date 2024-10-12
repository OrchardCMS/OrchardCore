using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Deployment;

public sealed class DeploymentPlanDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<DeploymentPlanDeploymentStep>
{
    private readonly IDeploymentPlanService _deploymentPlanService;

    public DeploymentPlanDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _deploymentPlanService = serviceProvider.GetService<IDeploymentPlanService>();
    }

    public override IDisplayResult Edit(DeploymentPlanDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<DeploymentPlanDeploymentStepViewModel>("DeploymentPlanDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.DeploymentPlanNames = step.DeploymentPlanNames;
            model.AllDeploymentPlanNames = (await _deploymentPlanService.GetAllDeploymentPlanNamesAsync()).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(DeploymentPlanDeploymentStep step, UpdateEditorContext context)
    {
        step.DeploymentPlanNames = [];

        await context.Updater.TryUpdateModelAsync(step,
                                          Prefix,
                                          x => x.DeploymentPlanNames,
                                          x => x.IncludeAll);

        // Don't have the selected option if include all.
        if (step.IncludeAll)
        {
            step.DeploymentPlanNames = [];
        }

        return Edit(step, context);
    }
}
