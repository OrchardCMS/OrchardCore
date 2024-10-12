using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Core.Deployment;

public sealed class ElasticIndexDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<ElasticIndexDeploymentStep>
{
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

    public ElasticIndexDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _elasticIndexSettingsService = serviceProvider.GetService<ElasticIndexSettingsService>();
    }

    public override IDisplayResult Edit(ElasticIndexDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<ElasticIndexDeploymentStepViewModel>(EditShape, async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.IndexNames;
            model.AllIndexNames = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ElasticIndexDeploymentStep step, UpdateEditorContext context)
    {
        step.IndexNames = [];

        await context.Updater.TryUpdateModelAsync(step,
                                          Prefix,
                                          x => x.IndexNames,
                                          x => x.IncludeAll);

        // Don't have the selected option if include all.
        if (step.IncludeAll)
        {
            step.IndexNames = [];
        }

        return Edit(step, context);
    }
}
