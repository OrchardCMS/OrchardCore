using System.Threading.Tasks;
using OrchardCore.Queries.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using Microsoft.Extensions.Localization;
using OrchardCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace OrchardCore.Queries.Deployment
{
    public class QueryBasedContentDeploymentStepDriver : DisplayDriver<DeploymentStep, QueryBasedContentDeploymentStep>
    {
        private readonly IQueryManager _queryManager;
        private readonly IStringLocalizer S;

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
            var model = new QueryBasedContentDeploymentStepViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, x => x.QueryName, x => x.QueryParameters, x => x.ExportAsSetupRecipe))
            {
                dynamic query = await _queryManager.LoadQueryAsync(model.QueryName);
                if (query.Source == "Lucene" && !query.ReturnContentItems)
                {
                    updater.ModelState.AddModelError(Prefix, nameof(step.QueryName), S["Your Lucene query is not returning content items."]);
                }
                else if (query.Source == "Sql" && !query.ReturnDocuments)
                {
                    updater.ModelState.AddModelError(Prefix, nameof(step.QueryName), S["Your SQL query is not returning documents."]);
                }

                if (model.QueryParameters != null)
                {
                    try
                    {
                        var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(model.QueryParameters);
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

                step.QueryName = model.QueryName;
                step.ExportAsSetupRecipe = model.ExportAsSetupRecipe;
                step.QueryParameters = model.QueryParameters;
            }

            return Edit(step);
        }
    }
}
