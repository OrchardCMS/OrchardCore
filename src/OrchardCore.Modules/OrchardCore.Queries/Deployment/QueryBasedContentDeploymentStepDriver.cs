using System.Text.Json;
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
                View("QueryBasedContentDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
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

        var query = await _queryManager.GetQueryAsync(queryBasedContentViewModel.QueryName);

        if (!query.CanReturnContentItems || !query.ReturnContentItems)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(step.QueryName), S["Your Query is not returning content items."]);
        }

        if (queryBasedContentViewModel.QueryParameters != null)
        {
            try
            {
                var parameters = JConvert.DeserializeObject<Dictionary<string, object>>(queryBasedContentViewModel.QueryParameters);
                if (parameters == null)
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(step.QueryParameters), S["Make sure it is a valid JSON object. Example: { key : 'value' }"]);
                }
            }
            catch (JsonException)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(step.QueryParameters), S["Something is wrong with your JSON."]);
            }
        }

        step.QueryName = queryBasedContentViewModel.QueryName;
        step.ExportAsSetupRecipe = queryBasedContentViewModel.ExportAsSetupRecipe;
        step.QueryParameters = queryBasedContentViewModel.QueryParameters;

        return Edit(step, context);
    }
}
