using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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
        private readonly GitHubAuthenticationSettings _gitHubAuthenticationSettings;

        public GitHubAuthenticationSettingsStep(
            IGitHubAuthenticationService githubLoginService,
            IOptions<GitHubAuthenticationSettings> gitHubAuthenticationSettings)
        {
            _githubAuthenticationService = githubLoginService;
            _gitHubAuthenticationSettings = gitHubAuthenticationSettings.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, nameof(GitHubAuthenticationSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var model = context.Step.ToObject<GitHubLoginSettingsStepModel>();

            _gitHubAuthenticationSettings.ClientID = model.ConsumerKey;
            _gitHubAuthenticationSettings.ClientSecret = model.ConsumerSecret;
            _gitHubAuthenticationSettings.CallbackPath = model.CallbackPath;

            await _githubAuthenticationService.UpdateSettingsAsync(_gitHubAuthenticationSettings);
        }
    }

    public class GitHubLoginSettingsStepModel
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string CallbackPath { get; set; }
    }
}
