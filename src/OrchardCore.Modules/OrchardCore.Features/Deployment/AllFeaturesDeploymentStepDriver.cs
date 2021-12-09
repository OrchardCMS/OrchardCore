using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Features.ViewModels;

namespace OrchardCore.Features.Deployment
{
    public class AllFeaturesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllFeaturesDeploymentStep>
    {
        public override IDisplayResult Display(AllFeaturesDeploymentStep step)
        {
            return
                Combine(
                    View("AllFeaturesDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("AllFeaturesDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllFeaturesDeploymentStep step)
        {
            return Initialize<AllFeaturesDeploymentStepViewModel>("AllFeaturesDeploymentStep_Fields_Edit", model =>
            {
                model.IgnoreDisabledFeatures = step.IgnoreDisabledFeatures;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(AllFeaturesDeploymentStep step, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(step, Prefix, x => x.IgnoreDisabledFeatures);

            return Edit(step);
        }
    }
}
