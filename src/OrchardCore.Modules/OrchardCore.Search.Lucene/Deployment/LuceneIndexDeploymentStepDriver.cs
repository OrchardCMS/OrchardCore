using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Lucene.ViewModels;

namespace OrchardCore.Search.Lucene.Deployment;

public sealed class LuceneIndexDeploymentStepDriver : DisplayDriver<DeploymentStep, LuceneIndexDeploymentStep>
{
    private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

    public LuceneIndexDeploymentStepDriver(LuceneIndexSettingsService luceneIndexSettingsService)
    {
        _luceneIndexSettingsService = luceneIndexSettingsService;
    }

    public override Task<IDisplayResult> DisplayAsync(LuceneIndexDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("LuceneIndexDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("LuceneIndexDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
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
