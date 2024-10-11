using System.Text.Json.Nodes;
using OrchardCore.Facebook.Login.Services;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Facebook.Login.Recipes;

/// <summary>
/// This recipe step sets general Facebook Login settings.
/// </summary>
public sealed class FacebookLoginSettingsStep : NamedRecipeStepHandler
{
    private readonly IFacebookLoginService _loginService;

    public FacebookLoginSettingsStep(IFacebookLoginService loginService)
        : base(nameof(FacebookLoginSettings))
    {
        _loginService = loginService;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<FacebookLoginSettingsStepModel>();
        var settings = await _loginService.LoadSettingsAsync();

        settings.CallbackPath = model.CallbackPath;

        await _loginService.UpdateSettingsAsync(settings);
    }
}

public sealed class FacebookLoginSettingsStepModel
{
    public string CallbackPath { get; set; }
}
