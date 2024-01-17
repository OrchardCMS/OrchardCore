using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Deployment.Steps
{
    public class JsonRecipeDeploymentStepDriver : DisplayDriver<DeploymentStep, JsonRecipeDeploymentStep>
    {
        /// <summary>
        /// A limited schema for recipe steps. Does not include any step data.
        /// </summary>
        public const string Schema = @"
{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""title"": ""JSON Recipe deployment plan"",
  ""properties"": {
    ""name"": {
      ""type"": ""string""
    }
  },
  ""required"": [
    ""name""
  ]
}
";

        protected readonly IStringLocalizer S;

        public JsonRecipeDeploymentStepDriver(IStringLocalizer<JsonRecipeDeploymentStepDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Display(JsonRecipeDeploymentStep step)
        {
            return
                Combine(
                    View("JsonRecipeDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("JsonRecipeDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(JsonRecipeDeploymentStep step)
        {
            return Initialize<JsonRecipeDeploymentStepViewModel>("JsonRecipeDeploymentStep_Fields_Edit", model =>
            {
                model.Json = step.Json;
                model.Schema = Schema;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(JsonRecipeDeploymentStep step, IUpdateModel updater)
        {
            var model = new JsonRecipeDeploymentStepViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix))
            {
                try
                {
                    var jObject = JObject.Parse(model.Json);
                    if (!jObject.ContainsKey("name"))
                    {

                        updater.ModelState.AddModelError(Prefix, nameof(JsonRecipeDeploymentStepViewModel.Json), S["The recipe must have a name property"]);
                    }

                }
                catch (Exception)
                {
                    updater.ModelState.AddModelError(Prefix, nameof(JsonRecipeDeploymentStepViewModel.Json), S["Invalid JSON supplied"]);

                }
                step.Json = model.Json;
            }

            return Edit(step);
        }
    }
}
