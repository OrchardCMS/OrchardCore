using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
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

        public override IDisplayResult Display(LuceneIndexResetDeploymentStep step)
        {
            return
                Combine(
                    View("LuceneIndexResetDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("LuceneIndexResetDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(LuceneIndexResetDeploymentStep step)
        {
            return Initialize<LuceneIndexResetDeploymentStepViewModel>("LuceneIndexResetDeploymentStep_Fields_Edit", async model =>
            {
                model.IncludeAll = step.IncludeAll;
                model.IndexNames = step.IndexNames;
                model.AllIndexNames = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(LuceneIndexResetDeploymentStep resetIndexStep, IUpdateModel updater)
        {
            resetIndexStep.IndexNames = Array.Empty<string>();

            await updater.TryUpdateModelAsync(resetIndexStep, Prefix, step => step.IndexNames, step => step.IncludeAll);

            if (resetIndexStep.IncludeAll)
            {
                resetIndexStep.IndexNames = Array.Empty<string>();
            }

            return Edit(resetIndexStep);
        }
    }
}
