using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Queries.ViewModels;

namespace OrchardCore.Queries.Deployment
{
    public class QueryBasedContentDeploymentStepDriver : DisplayDriver<DeploymentStep, QueryBasedContentDeploymentStep>
    {
        private readonly IQueryManager _queryManager;
        protected readonly IStringLocalizer S;

        public QueryBasedContentDeploymentStepDriver(
            IQueryManager queryManager,
            IStringLocalizer<QueryBasedContentDeploymentStepDriver> stringLocalizer)
        {
            _queryManager = queryManager;
            S = stringLocalizer;
        }

        public override IDisplayResult Display(QueryBasedContentDeploymentStep step)
        {
            return
                Combine(
                    View("QueryBasedContentDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("QueryBasedContentDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(QueryBasedContentDeploymentStep step)
        {
            return Initialize<QueryBasedContentDeploymentStepViewModel>("QueryBasedContentDeploymentStep_Fields_Edit", model =>
            {
                model.QueryName = step.QueryName;
                model.QueryParameters = step.QueryParameters;
                model.ExportAsSetupRecipe = step.ExportAsSetupRecipe;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(QueryBasedContentDeploymentStep step, IUpdateModel updater)
        {
            var queryBasedContentViewModel = new QueryBasedContentDeploymentStepViewModel();

            if (await updater.TryUpdateModelAsync(queryBasedContentViewModel, Prefix, viewModel => viewModel.QueryName, viewModel => viewModel.QueryParameters, viewModel => viewModel.ExportAsSetupRecipe))
            {
                var query = await _queryManager.LoadQueryAsync(queryBasedContentViewModel.QueryName);
                if (!query.ResultsOfType<ContentItem>())
                {
                    updater.ModelState.AddModelError(Prefix, nameof(step.QueryName), S["Your Query is not returning content items."]);
                }

                if (queryBasedContentViewModel.QueryParameters != null)
                {
                    try
                    {
                        var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(queryBasedContentViewModel.QueryParameters);
                        if (parameters == null)
                        {
                            updater.ModelState.AddModelError(Prefix, nameof(step.QueryParameters), S["Make sure it is a valid JSON object. Example: { key : 'value' }"]);
                        }
                    }
                    catch (JsonException)
                    {
                        updater.ModelState.AddModelError(Prefix, nameof(step.QueryParameters), S["Something is wrong with your JSON."]);
                    }
                }

                step.QueryName = queryBasedContentViewModel.QueryName;
                step.ExportAsSetupRecipe = queryBasedContentViewModel.ExportAsSetupRecipe;
                step.QueryParameters = queryBasedContentViewModel.QueryParameters;
            }

            return Edit(step);
        }
    }
}
