using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment
{
    public class ElasticIndexResetDeploymentStepDriver : DisplayDriver<DeploymentStep, ElasticIndexResetDeploymentStep>
    {
        private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

        public ElasticIndexResetDeploymentStepDriver(ElasticIndexSettingsService elasticIndexSettingsService)
        {
            _elasticIndexSettingsService = elasticIndexSettingsService;
        }

        public override IDisplayResult Display(ElasticIndexResetDeploymentStep step)
        {
            return
                Combine(
                    View("ElasticIndexResetDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("ElasticIndexResetDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(ElasticIndexResetDeploymentStep step)
        {
            return Initialize<ElasticIndexResetDeploymentStepViewModel>("ElasticIndexResetDeploymentStep_Fields_Edit", async model =>
            {
                model.IncludeAll = step.IncludeAll;
                model.IndexNames = step.Indices;
                model.AllIndexNames = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ElasticIndexResetDeploymentStep resetIndexStep, IUpdateModel updater)
        {
            resetIndexStep.Indices = Array.Empty<string>();

            await updater.TryUpdateModelAsync(resetIndexStep, Prefix, step => step.Indices, step => step.IncludeAll);

            if (resetIndexStep.IncludeAll)
            {
                resetIndexStep.Indices = Array.Empty<string>();
            }

            return Edit(resetIndexStep);
        }
    }
}
