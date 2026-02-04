using OrchardCore.Recipes.Schema;
using OrchardCore.Facebook.Login.Services;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Facebook.Login.Recipes;

public sealed class FacebookLoginSettingsRecipeStep : RecipeImportStep<FacebookLoginSettingsStepModel>
{
    private readonly IFacebookLoginService _loginService;

    public FacebookLoginSettingsRecipeStep(IFacebookLoginService loginService)
    {
        _loginService = loginService;
    }

    public override string Name => nameof(FacebookLoginSettings);

    protected override RecipeStepSchema BuildSchema()
        => RecipeStepSchema.Any;

    protected override async Task ImportAsync(FacebookLoginSettingsStepModel model, RecipeExecutionContext context)
    {
        var settings = await _loginService.LoadSettingsAsync();

        settings.CallbackPath = model.CallbackPath;

        await _loginService.UpdateSettingsAsync(settings);
    }
}
