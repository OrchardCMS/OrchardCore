using System;
using System.Threading.Tasks;
using OrchardCore.Queries.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Queries.Deployment
{
    public class QueryBasedContentDeploymentStepDriver : DisplayDriver<DeploymentStep, QueryBasedContentDeploymentStep>
    {
        public override IDisplayResult Display(QueryBasedContentDeploymentStep step)
        {
            return
                Combine(
                    View("QueryBasedContentDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("QueryBasedContentDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(QueryBasedContentDeploymentStep step)
        {
            return Initialize<QueryBasedContentDeploymentStepViewModel>("QueryBasedContentDeploymentStep_Fields_Edit", model =>
            {
                model.QueryName = step.QueryName;
                model.QueryParameters = step.QueryParameters;
                model.ExportAsSetupRecipe = step.ExportAsSetupRecipe;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(QueryBasedContentDeploymentStep step, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(step, Prefix, x => x.QueryName, x => x.QueryParameters, x => x.ExportAsSetupRecipe);

            return Edit(step);
        }
    }
}
