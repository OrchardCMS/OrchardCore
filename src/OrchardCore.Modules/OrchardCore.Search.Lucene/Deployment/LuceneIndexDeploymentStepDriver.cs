using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Lucene.ViewModels;

namespace OrchardCore.Search.Lucene.Deployment;

public sealed class LuceneIndexDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<LuceneIndexDeploymentStep>
{
    private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

    public LuceneIndexDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _luceneIndexSettingsService = serviceProvider.GetService<LuceneIndexSettingsService>();
    }

    public override IDisplayResult Edit(LuceneIndexDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<LuceneIndexDeploymentStepViewModel>("LuceneIndexDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.IndexNames;
            model.AllIndexNames = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(LuceneIndexDeploymentStep step, UpdateEditorContext context)
    {
        step.IndexNames = [];

        await context.Updater.TryUpdateModelAsync(step,
                                          Prefix,
                                          x => x.IndexNames,
                                          x => x.IncludeAll);

        // don't have the selected option if include all
        if (step.IncludeAll)
        {
            step.IndexNames = [];
        }

        return Edit(step, context);
    }
}
