using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
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

        public override Task<IDisplayResult> DisplayAsync(DeploymentPlanDeploymentStep step, BuildDisplayContext context)
        {
            return
                CombineAsync(
                    View("DeploymentPlanDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("DeploymentPlanDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override Task<IDisplayResult> EditAsync(DeploymentPlanDeploymentStep step, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<DeploymentPlanDeploymentStepViewModel>("DeploymentPlanDeploymentStep_Fields_Edit", async model =>
                {
                    model.IncludeAll = step.IncludeAll;
                    model.DeploymentPlanNames = step.DeploymentPlanNames;
                    model.AllDeploymentPlanNames = (await _deploymentPlanService.GetAllDeploymentPlanNamesAsync()).ToArray();
                }).Location("Content")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(DeploymentPlanDeploymentStep step, UpdateEditorContext context)
        {
            step.DeploymentPlanNames = [];

            await context.Updater.TryUpdateModelAsync(step,
                                              Prefix,
                                              x => x.DeploymentPlanNames,
                                              x => x.IncludeAll);

            // don't have the selected option if include all
            if (step.IncludeAll)
            {
                step.DeploymentPlanNames = [];
            }

            return await EditAsync(step, context);
        }
    }
}
