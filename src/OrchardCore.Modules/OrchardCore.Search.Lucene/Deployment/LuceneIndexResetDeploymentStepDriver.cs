using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Lucene.ViewModels;

namespace OrchardCore.Search.Lucene.Deployment
{
    public class LuceneIndexResetDeploymentStepDriver : DisplayDriver<DeploymentStep, LuceneIndexResetDeploymentStep>
    {
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

        public LuceneIndexResetDeploymentStepDriver(LuceneIndexSettingsService luceneIndexSettingsService)
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
        }

        public override Task<IDisplayResult> DisplayAsync(LuceneIndexResetDeploymentStep step, BuildDisplayContext context)
        {
            return
                CombineAsync(
                    View("LuceneIndexResetDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("LuceneIndexResetDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override Task<IDisplayResult> EditAsync(LuceneIndexResetDeploymentStep step, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<LuceneIndexResetDeploymentStepViewModel>("LuceneIndexResetDeploymentStep_Fields_Edit", async model =>
                {
                    model.IncludeAll = step.IncludeAll;
                    model.IndexNames = step.IndexNames;
                    model.AllIndexNames = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
                }).Location("Content")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(LuceneIndexResetDeploymentStep resetIndexStep, UpdateEditorContext context)
        {
            resetIndexStep.IndexNames = [];

            await context.Updater.TryUpdateModelAsync(resetIndexStep, Prefix, step => step.IndexNames, step => step.IncludeAll);

            if (resetIndexStep.IncludeAll)
            {
                resetIndexStep.IndexNames = [];
            }

            return await EditAsync(resetIndexStep, context);
        }
    }
}
