using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public sealed class ElasticIndexResetDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<ElasticIndexResetDeploymentStep, ElasticIndexResetDeploymentStepViewModel>
{
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

    public ElasticIndexResetDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _elasticIndexSettingsService = serviceProvider.GetService<ElasticIndexSettingsService>();
    }

    public override IDisplayResult Edit(ElasticIndexResetDeploymentStep step, Action<ElasticIndexResetDeploymentStepViewModel> intializeAction)
    {
        return base.Edit(step, async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indices;
            model.AllIndexNames = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(ElasticIndexResetDeploymentStep resetIndexStep, UpdateEditorContext context)
    {
        resetIndexStep.Indices = [];

        await context.Updater.TryUpdateModelAsync(resetIndexStep, Prefix, step => step.Indices, step => step.IncludeAll);

        if (resetIndexStep.IncludeAll)
        {
            resetIndexStep.Indices = [];
        }

        return Edit(resetIndexStep, context);
    }
}
