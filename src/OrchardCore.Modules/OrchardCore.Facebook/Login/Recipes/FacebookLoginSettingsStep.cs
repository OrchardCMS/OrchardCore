using System.Text.Json.Nodes;
using OrchardCore.Environment.Options;
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
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;

    public FacebookLoginSettingsStep(IFacebookLoginService loginService, IOptionsUpdateNotifier optionsUpdateNotifier)
        : base(nameof(FacebookLoginSettings))
    {
        _loginService = loginService;
        _optionsUpdateNotifier = optionsUpdateNotifier;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<FacebookLoginSettingsStepModel>();
        var settings = await _loginService.LoadSettingsAsync();

        settings.CallbackPath = model.CallbackPath;

        await _loginService.UpdateSettingsAsync(settings);

        _optionsUpdateNotifier.RequestFacebookLoginUpdate();
    }
}

public sealed class FacebookLoginSettingsStepModel
{
    public string CallbackPath { get; set; }
}
