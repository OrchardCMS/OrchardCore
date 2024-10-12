using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Lucene.ViewModels;

namespace OrchardCore.Search.Lucene.Deployment;

public sealed class LuceneIndexResetDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<LuceneIndexResetDeploymentStep>
{
    private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

    public LuceneIndexResetDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _luceneIndexSettingsService= serviceProvider.GetService<LuceneIndexSettingsService>();
    }

    public override IDisplayResult Edit(LuceneIndexResetDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<LuceneIndexResetDeploymentStepViewModel>("LuceneIndexResetDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.IndexNames;
            model.AllIndexNames = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(LuceneIndexResetDeploymentStep resetIndexStep, UpdateEditorContext context)
    {
        resetIndexStep.IndexNames = [];

        await context.Updater.TryUpdateModelAsync(resetIndexStep, Prefix, step => step.IndexNames, step => step.IncludeAll);

        if (resetIndexStep.IncludeAll)
        {
            resetIndexStep.IndexNames = [];
        }

        return Edit(resetIndexStep, context);
    }
}
