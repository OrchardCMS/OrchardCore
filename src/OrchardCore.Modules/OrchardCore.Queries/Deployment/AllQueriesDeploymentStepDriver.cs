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
                    Shape("AllQueriesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    Shape("AllQueriesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllQueriesDeploymentStep step)
        {
            return Shape("AllQueriesDeploymentStep_Edit", step).Location("Content");
        }
    }
}
