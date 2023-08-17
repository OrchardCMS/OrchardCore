using System;
using System.Threading.Tasks;
using OrchardCore.GitHub.Services;
using OrchardCore.GitHub.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.GitHub.Recipes
{
    /// <summary>
    /// This recipe step sets GitHub Account settings.
    /// </summary>
    public class GitHubAuthenticationSettingsStep : IRecipeStepHandler
    {
        private readonly IGitHubAuthenticationService _githubAuthenticationService;

        public GitHubAuthenticationSettingsStep(IGitHubAuthenticationService githubLoginService)
        {
            _githubAuthenticationService = githubLoginService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, nameof(GitHubAuthenticationSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var model = context.Step.ToObject<GitHubLoginSettingsStepModel>();
            var settings = await _githubAuthenticationService.LoadSettingsAsync();

            settings.ClientID = model.ConsumerKey;
            settings.ClientSecret = model.ConsumerSecret;
            settings.CallbackPath = model.CallbackPath;

            await _githubAuthenticationService.UpdateSettingsAsync(settings);
        }
    }

    public class GitHubLoginSettingsStepModel
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string CallbackPath { get; set; }
    }
}
