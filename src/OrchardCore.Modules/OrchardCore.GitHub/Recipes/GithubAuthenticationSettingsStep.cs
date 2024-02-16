using System;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public GitHubAuthenticationSettingsStep(
            IGitHubAuthenticationService githubLoginService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _githubAuthenticationService = githubLoginService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(GitHubAuthenticationSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var model = context.Step.ToObject<GitHubLoginSettingsStepModel>(_jsonSerializerOptions);
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
