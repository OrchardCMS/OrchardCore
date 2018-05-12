using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Queries.Deployment
{
    public class AllQueriesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllQueriesDeploymentStep>
    {
        public override IDisplayResult Display(AllQueriesDeploymentStep step)
        {
            return
                Combine(
                    View("AllQueriesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllQueriesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllQueriesDeploymentStep step)
        {
            return View("AllQueriesDeploymentStep_Edit", step).Location("Content");
        }
    }
}
