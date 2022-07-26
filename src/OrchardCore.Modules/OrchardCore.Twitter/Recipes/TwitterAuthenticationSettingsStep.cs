using System;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Twitter.Services;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Recipes
{
    /// <summary>
    /// This recipe step sets Twitter Account settings.
    /// </summary>
    public class TwitterAuthenticationSettingsStep : IRecipeStepHandler
    {
        private readonly ITwitterAuthenticationService _twitterService;

        public TwitterAuthenticationSettingsStep(ITwitterAuthenticationService twitterService)
        {
            _twitterService = twitterService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, nameof(TwitterAuthenticationSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<TwitterAuthenticationSettingsStepModel>();
            var settings = await _twitterService.LoadSettingsAsync();

            settings.ConsumerKey = model.ConsumerKey;
            settings.ConsumerSecret = model.ConsumerSecret;
            settings.AccessToken = model.AccessToken;
            settings.AccessTokenSecret = model.AccessTokenSecret;
            settings.CallbackPath = model.CallbackPath;

            await _twitterService.UpdateSettingsAsync(settings);
        }
    }

    public class TwitterAuthenticationSettingsStepModel
    {
        public string ConsumerKey { get; set; }

        public string ConsumerSecret { get; set; }

        public string AccessToken { get; set; }

        public string AccessTokenSecret { get; set; }

        public string CallbackPath { get; set; }
    }
}
