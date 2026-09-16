using Microsoft.Extensions.Localization;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.Deployment.Services;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Deployment.Steps;

public sealed class JsonRecipeDeploymentStepDriver : DisplayDriver<DeploymentStep, JsonRecipeDeploymentStep>
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

    internal readonly IStringLocalizer S;

    public JsonRecipeDeploymentStepDriver(IStringLocalizer<JsonRecipeDeploymentStepDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(JsonRecipeDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("JsonRecipeDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("JsonRecipeDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(JsonRecipeDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<JsonRecipeDeploymentStepViewModel>("JsonRecipeDeploymentStep_Fields_Edit", model =>
        {
            model.Json = step.Json;
            model.Schema = Schema;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(JsonRecipeDeploymentStep step, UpdateEditorContext context)
    {
        var model = new JsonRecipeDeploymentStepViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var candidate = new JsonRecipeDeploymentStep { Json = model.Json };
        var errors = DeploymentStepValidation.Validate(candidate);
        foreach (var messages in errors.Values)
        {
            foreach (var message in messages)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(JsonRecipeDeploymentStepViewModel.Json), S[message]);
            }
        }
        if (errors.Count == 0)
        {
            step.Json = candidate.Json;
        }

        return Edit(step, context);
    }
}
