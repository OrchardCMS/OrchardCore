using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Search.Lucene.Deployment
{
    public class LuceneSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, LuceneSettingsDeploymentStep>
    {
        public override Task<IDisplayResult> DisplayAsync(LuceneSettingsDeploymentStep step, BuildDisplayContext context)
        {
            return
                CombineAsync(
                    View("LuceneSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("LuceneSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override Task<IDisplayResult> EditAsync(LuceneSettingsDeploymentStep step, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                View("LuceneSettingsDeploymentStep_Fields_Edit", step).Location("Content")
            );
        }
    }
}
