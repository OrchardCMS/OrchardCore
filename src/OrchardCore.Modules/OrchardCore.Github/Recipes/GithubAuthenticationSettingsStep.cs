using System;
using System.Threading.Tasks;
using OrchardCore.Github.Services;
using OrchardCore.Github.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Github.Recipes
{
    /// <summary>
    /// This recipe step sets Github Account settings.
    /// </summary>
    public class GithubAuthenticationSettingsStep : IRecipeStepHandler
    {
        private readonly IGithubAuthenticationService _githubAuthenticationService;

        public GithubAuthenticationSettingsStep(IGithubAuthenticationService githubLoginService)
        {
            _githubAuthenticationService = githubLoginService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(GithubAuthenticationSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var model = context.Step.ToObject<GithubLoginSettingsStepModel>();
            var settings = await _githubAuthenticationService.GetSettingsAsync();
            settings.ClientID = model.ConsumerKey;
            settings.ClientSecret = model.ConsumerSecret;
            settings.CallbackPath = model.CallbackPath;
            await _githubAuthenticationService.UpdateSettingsAsync(settings);
        }
    }

    public class GithubLoginSettingsStepModel
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string CallbackPath { get; set; }
    }
}