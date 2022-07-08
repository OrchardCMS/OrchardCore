using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Deployment
{
    public class DeploymentPlanDeploymentStepDriver : DisplayDriver<DeploymentStep, DeploymentPlanDeploymentStep>
    {
        private readonly IDeploymentPlanService _deploymentPlanService;

        public DeploymentPlanDeploymentStepDriver(IDeploymentPlanService deploymentPlanService)
        {
            _deploymentPlanService = deploymentPlanService;
        }

        public override IDisplayResult Display(DeploymentPlanDeploymentStep step)
        {
            return
                Combine(
                    View("DeploymentPlanDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("DeploymentPlanDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(DeploymentPlanDeploymentStep step)
        {
            return Initialize<DeploymentPlanDeploymentStepViewModel>("DeploymentPlanDeploymentStep_Fields_Edit", async model =>
            {
                model.IncludeAll = step.IncludeAll;
                model.DeploymentPlanNames = step.DeploymentPlanNames;
                model.AllDeploymentPlanNames = (await _deploymentPlanService.GetAllDeploymentPlanNamesAsync()).ToArray();
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(DeploymentPlanDeploymentStep step, IUpdateModel updater)
        {
            step.DeploymentPlanNames = Array.Empty<string>();

            await updater.TryUpdateModelAsync(step,
                                              Prefix,
                                              x => x.DeploymentPlanNames,
                                              x => x.IncludeAll);

            // don't have the selected option if include all
            if (step.IncludeAll)
            {
                step.DeploymentPlanNames = Array.Empty<string>();
            }

            return Edit(step);
        }
    }
}
