using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Rules.Drivers
{
    public class AllRuleDisplayDriver : DisplayDriver<Rule, AllRule>
    {

        public override IDisplayResult Display(AllRule rule)
        {
            return
                Combine(
                    View("AllRule_Fields_Summary", rule).Location("Summary", "Content"),
                    View("AllRule_Fields_Thumbnail", rule).Location("Thumbnail", "Content")
                );
        }

        // public override IDisplayResult Edit(AllRule step)
        // {
        //     return Initialize<DeploymentPlanDeploymentStepViewModel>("AllRule_Fields_Edit", async model =>
        //     {
        //         model.IncludeAll = step.IncludeAll;
        //         model.DeploymentPlanNames = step.DeploymentPlanNames;
        //         model.AllDeploymentPlanNames = (await _deploymentPlanService.GetAllDeploymentPlanNamesAsync()).ToArray();
        //     }).Location("Content");
        // }

        // public override async Task<IDisplayResult> UpdateAsync(DeploymentPlanDeploymentStep step, IUpdateModel updater)
        // {
        //     step.DeploymentPlanNames = Array.Empty<string>();

        //     await updater.TryUpdateModelAsync(step,
        //                                       Prefix,
        //                                       x => x.DeploymentPlanNames,
        //                                       x => x.IncludeAll);

        //     // don't have the selected option if include all
        //     if (step.IncludeAll)
        //     {
        //         step.DeploymentPlanNames = Array.Empty<string>();
        //     }

        //     return Edit(step);
        // }
    }
}
