using System.Text.Json.Nodes;
using OrchardCore.Facebook.Login.Services;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Facebook.Login.Recipes;

/// <summary>
/// This recipe step sets general Facebook Login settings.
/// </summary>
[Obsolete("Implement IRecipeDeploymentStep instead. This class will be removed in a future version.", false)]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed class FacebookLoginSettingsStep : NamedRecipeStepHandler
#pragma warning restore CS0618
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
