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
    public class LuceneIndexRebuildDeploymentStepDriver : DisplayDriver<DeploymentStep, LuceneIndexRebuildDeploymentStep>
    {
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

        public LuceneIndexRebuildDeploymentStepDriver(LuceneIndexSettingsService luceneIndexSettingsService)
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
        }

        public override IDisplayResult Display(LuceneIndexRebuildDeploymentStep step)
        {
            return
                Combine(
                    View("LuceneIndexRebuildDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("LuceneIndexRebuildDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(LuceneIndexRebuildDeploymentStep step)
        {
            return Initialize<LuceneIndexRebuildDeploymentStepViewModel>("LuceneIndexRebuildDeploymentStep_Fields_Edit", async model =>
            {
                model.IncludeAll = step.IncludeAll;
                model.IndexNames = step.IndexNames;
                model.AllIndexNames = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(LuceneIndexRebuildDeploymentStep rebuildIndexStep, IUpdateModel updater)
        {
            rebuildIndexStep.IndexNames = Array.Empty<string>();

            await updater.TryUpdateModelAsync(rebuildIndexStep, Prefix, step => step.IndexNames, step => step.IncludeAll);

            if (rebuildIndexStep.IncludeAll)
            {
                rebuildIndexStep.IndexNames = Array.Empty<string>();
            }

            return Edit(rebuildIndexStep);
        }
    }
}
