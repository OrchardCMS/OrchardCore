using System.Text.Json.Nodes;
using OrchardCore.Entities;
using OrchardCore.GitHub.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Recipes;

/// <summary>
/// This recipe step sets GitHub Account settings.
/// </summary>
public sealed class GitHubAuthenticationSettingsStep : NamedRecipeStepHandler
{
    private readonly ISiteService _siteService;

    public GitHubAuthenticationSettingsStep(ISiteService siteService)
        : base(nameof(GitHubAuthenticationSettings))
    {
        _siteService = siteService;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<GitHubLoginSettingsStepModel>();
        var site = await _siteService.LoadSiteSettingsAsync();

        site.Alter<GitHubAuthenticationSettings>(settings =>
        {
            settings.ClientID = model.ConsumerKey;
            settings.ClientSecret = model.ConsumerSecret;
            settings.CallbackPath = model.CallbackPath;
        });

        await _siteService.UpdateSiteSettingsAsync(site);
    }
}

public sealed class GitHubLoginSettingsStepModel
{
    public string ConsumerKey { get; set; }

    public string ConsumerSecret { get; set; }

    public string CallbackPath { get; set; }
}
