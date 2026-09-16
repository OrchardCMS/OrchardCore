using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Queries.ViewModels;

namespace OrchardCore.Queries.Deployment;

public sealed class QueryBasedContentDeploymentStepDriver : DisplayDriver<DeploymentStep, QueryBasedContentDeploymentStep>
{
    private readonly IQueryManager _queryManager;

    internal readonly IStringLocalizer S;

    public QueryBasedContentDeploymentStepDriver(
        IQueryManager queryManager,
        IStringLocalizer<QueryBasedContentDeploymentStepDriver> stringLocalizer)
    {
        _queryManager = queryManager;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(QueryBasedContentDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("QueryBasedContentDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("QueryBasedContentDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(QueryBasedContentDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<QueryBasedContentDeploymentStepViewModel>("QueryBasedContentDeploymentStep_Fields_Edit", async model =>
        {
            model.QueryName = step.QueryName;
            model.QueryParameters = step.QueryParameters;
            model.ExportAsSetupRecipe = step.ExportAsSetupRecipe;
            model.Queries = await _queryManager.ListQueriesAsync(true);
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(QueryBasedContentDeploymentStep step, UpdateEditorContext context)
    {
        var queryBasedContentViewModel = new QueryBasedContentDeploymentStepViewModel();
        await context.Updater.TryUpdateModelAsync(queryBasedContentViewModel, Prefix,
            viewModel => viewModel.QueryName,
            viewModel => viewModel.QueryParameters,
            viewModel => viewModel.ExportAsSetupRecipe);

        var errors = await QueryDeploymentConfiguration.ValidateAsync(_queryManager,
            queryBasedContentViewModel.QueryName, queryBasedContentViewModel.QueryParameters);
        foreach (var (property, messages) in errors)
        {
            foreach (var message in messages)
            {
                context.Updater.ModelState.AddModelError(Prefix, property, S[message]);
            }
        }

        step.QueryName = queryBasedContentViewModel.QueryName;
        step.ExportAsSetupRecipe = queryBasedContentViewModel.ExportAsSetupRecipe;
        step.QueryParameters = queryBasedContentViewModel.QueryParameters;

        return Edit(step, context);
    }
}
