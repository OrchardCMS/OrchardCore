using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lucene.ViewModels;

namespace OrchardCore.Lucene.Deployment
{
    public class LuceneIndexDeploymentStepDriver : DisplayDriver<DeploymentStep, LuceneIndexDeploymentStep>
    {
        private readonly LuceneIndexManager _luceneIndexManager;

        public LuceneIndexDeploymentStepDriver(LuceneIndexManager luceneIndexManager)
        {
            _luceneIndexManager = luceneIndexManager;
        }

        public override IDisplayResult Display(LuceneIndexDeploymentStep step)
        {
            return
                Combine(
                    View("LuceneIndexDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("LuceneIndexDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(LuceneIndexDeploymentStep step)
        {
            return Initialize<LuceneIndexDeploymentStepViewModel>("LuceneIndexDeploymentStep_Fields_Edit", model =>
            {
                model.IncludeAll = step.IncludeAll;
                model.IndexNames = step.IndexNames;
                model.AllIndexNames = _luceneIndexManager.List().ToArray();
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(LuceneIndexDeploymentStep step, IUpdateModel updater)
        {
            step.IndexNames = Array.Empty<string>();

            await updater.TryUpdateModelAsync(step,
                                              Prefix,
                                              x => x.IndexNames,
                                              x => x.IncludeAll);

            // don't have the selected option if include all
            if (step.IncludeAll)
            {
                step.IndexNames = Array.Empty<string>();
            }

            return Edit(step);
        }
    }
}
