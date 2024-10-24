using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Lucene.ViewModels;

namespace OrchardCore.Search.Lucene.Deployment;

public sealed class LuceneIndexResetDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<LuceneIndexResetDeploymentStep, LuceneIndexResetDeploymentStepViewModel>
{
    private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

    public LuceneIndexResetDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _luceneIndexSettingsService= serviceProvider.GetService<LuceneIndexSettingsService>();
    }

    public override IDisplayResult Edit(LuceneIndexResetDeploymentStep step, Action<LuceneIndexResetDeploymentStepViewModel> intializeAction)
    {
        return base.Edit(step, async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.IndexNames;
            model.AllIndexNames = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
        });
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
