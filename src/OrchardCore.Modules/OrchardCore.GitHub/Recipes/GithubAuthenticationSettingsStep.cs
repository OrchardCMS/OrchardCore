using System.Text.Json.Nodes;
using OrchardCore.GitHub.Services;
using OrchardCore.GitHub.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.GitHub.Recipes;

/// <summary>
/// This recipe step sets GitHub Account settings.
/// </summary>
public sealed class GitHubAuthenticationSettingsStep : NamedRecipeStepHandler
{
    private readonly IGitHubAuthenticationService _githubAuthenticationService;

    public GitHubAuthenticationSettingsStep(IGitHubAuthenticationService githubLoginService)
        : base(nameof(GitHubAuthenticationSettings))
    {
        _githubAuthenticationService = githubLoginService;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<GitHubLoginSettingsStepModel>();
        var settings = await _githubAuthenticationService.LoadSettingsAsync();

        settings.ClientID = model.ConsumerKey;
        settings.ClientSecret = model.ConsumerSecret;
        settings.CallbackPath = model.CallbackPath;

        await _githubAuthenticationService.UpdateSettingsAsync(settings);
    }
}

public sealed class GitHubLoginSettingsStepModel
{
    public string ConsumerKey { get; set; }
    public string ConsumerSecret { get; set; }
    public string CallbackPath { get; set; }
}
