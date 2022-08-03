using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Deployment
{
    public class ElasticsearchIndexDeploymentStepDriver : DisplayDriver<DeploymentStep, ElasticsearchIndexDeploymentStep>
    {
        private readonly ElasticsearchIndexSettingsService _elasticIndexSettingsService;

        public ElasticsearchIndexDeploymentStepDriver(ElasticsearchIndexSettingsService elasticIndexSettingsService)
        {
            _elasticIndexSettingsService = elasticIndexSettingsService;
        }

        public override IDisplayResult Display(ElasticsearchIndexDeploymentStep step)
        {
            return
                Combine(
                    View("ElasticsearchIndexDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("ElasticsearchIndexDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(ElasticsearchIndexDeploymentStep step)
        {
            return Initialize<ElasticsearchIndexDeploymentStepViewModel>("ElasticsearchIndexDeploymentStep_Fields_Edit", async model =>
            {
                model.IncludeAll = step.IncludeAll;
                model.IndexNames = step.IndexNames;
                model.AllIndexNames = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ElasticsearchIndexDeploymentStep step, IUpdateModel updater)
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
