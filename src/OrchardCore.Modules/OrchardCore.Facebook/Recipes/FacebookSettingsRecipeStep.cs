using OrchardCore.Recipes.Schema;
using OrchardCore.Facebook.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Facebook.Recipes;

public sealed class FacebookSettingsRecipeStep : RecipeImportStep<FacebookCoreSettingsStepModel>
{
    private readonly IFacebookService _facebookService;

    public FacebookSettingsRecipeStep(IFacebookService facebookService)
    {
        _facebookService = facebookService;
    }

    public override string Name => "FacebookCoreSettings";

    protected override RecipeStepSchema BuildSchema()
        => RecipeStepSchema.Any;

    protected override async Task ImportAsync(FacebookCoreSettingsStepModel model, RecipeExecutionContext context)
    {
        var settings = await _facebookService.GetSettingsAsync();

        settings.AppId = model.AppId;
        settings.AppSecret = model.AppSecret;
        settings.SdkJs = model.SdkJs ?? "sdk.js";
        settings.FBInit = model.FBInit;
        settings.FBInitParams = model.FBInitParams;
        settings.Version = model.Version ?? "3.2";

        await _facebookService.UpdateSettingsAsync(settings);
    }
}
