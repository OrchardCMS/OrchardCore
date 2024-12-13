using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Lucene.ViewModels;

namespace OrchardCore.Search.Lucene.Deployment;

public sealed class LuceneIndexRebuildDeploymentStepDriver : DisplayDriver<DeploymentStep, LuceneIndexRebuildDeploymentStep>
{
    private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

    public LuceneIndexRebuildDeploymentStepDriver(LuceneIndexSettingsService luceneIndexSettingsService)
    {
        _luceneIndexSettingsService = luceneIndexSettingsService;
    }

    public override Task<IDisplayResult> DisplayAsync(LuceneIndexRebuildDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("LuceneIndexRebuildDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("LuceneIndexRebuildDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(LuceneIndexRebuildDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<LuceneIndexRebuildDeploymentStepViewModel>("LuceneIndexRebuildDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.IndexNames;
            model.AllIndexNames = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(LuceneIndexRebuildDeploymentStep rebuildIndexStep, UpdateEditorContext context)
    {
        rebuildIndexStep.IndexNames = [];

        await context.Updater.TryUpdateModelAsync(rebuildIndexStep, Prefix, step => step.IndexNames, step => step.IncludeAll);

        if (rebuildIndexStep.IncludeAll)
        {
            rebuildIndexStep.IndexNames = [];
        }

        return Edit(rebuildIndexStep, context);
    }
}
