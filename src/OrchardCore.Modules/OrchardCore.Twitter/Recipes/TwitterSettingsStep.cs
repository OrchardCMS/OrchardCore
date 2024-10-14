using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Twitter.Services;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Recipes;

/// <summary>
/// This recipe step sets Microsoft Account settings.
/// </summary>
public sealed class TwitterSettingsStep : NamedRecipeStepHandler
{
    private readonly ITwitterSettingsService _twitterService;

    public TwitterSettingsStep(ITwitterSettingsService twitterService)
        : base(nameof(TwitterSettings))
    {
        _twitterService = twitterService;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<TwitterSettingsStepModel>();
        var settings = await _twitterService.LoadSettingsAsync();

        settings.ConsumerKey = model.ConsumerKey;
        settings.ConsumerSecret = model.ConsumerSecret;
        settings.AccessToken = model.AccessToken;
        settings.AccessTokenSecret = model.AccessTokenSecret;

        await _twitterService.UpdateSettingsAsync(settings);
    }
}

public sealed class TwitterSettingsStepModel
{
    public string ConsumerKey { get; set; }
    public string ConsumerSecret { get; set; }
    public string AccessToken { get; set; }
    public string AccessTokenSecret { get; set; }
}
