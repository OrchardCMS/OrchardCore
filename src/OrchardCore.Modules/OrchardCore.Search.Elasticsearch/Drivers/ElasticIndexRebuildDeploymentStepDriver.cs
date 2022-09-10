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
    public class ElasticIndexRebuildDeploymentStepDriver : DisplayDriver<DeploymentStep, ElasticIndexRebuildDeploymentStep>
    {
        private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

        public ElasticIndexRebuildDeploymentStepDriver(ElasticIndexSettingsService elasticIndexSettingsService)
        {
            _elasticIndexSettingsService = elasticIndexSettingsService;
        }

        public override IDisplayResult Display(ElasticIndexRebuildDeploymentStep step)
        {
            return
                Combine(
                    View("ElasticIndexRebuildDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("ElasticIndexRebuildDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(ElasticIndexRebuildDeploymentStep step)
        {
            return Initialize<ElasticIndexRebuildDeploymentStepViewModel>("ElasticIndexRebuildDeploymentStep_Fields_Edit", async model =>
            {
                model.IncludeAll = step.IncludeAll;
                model.IndexNames = step.Indices;
                model.AllIndexNames = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ElasticIndexRebuildDeploymentStep rebuildIndexStep, IUpdateModel updater)
        {
            rebuildIndexStep.Indices = Array.Empty<string>();

            await updater.TryUpdateModelAsync(rebuildIndexStep, Prefix, step => step.Indices, step => step.IncludeAll);

            if (rebuildIndexStep.IncludeAll)
            {
                rebuildIndexStep.Indices = Array.Empty<string>();
            }

            return Edit(rebuildIndexStep);
        }
    }
}
