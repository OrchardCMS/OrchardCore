using System;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Twitter.Services;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Recipes
{
    /// <summary>
    /// This recipe step sets Microsoft Account settings.
    /// </summary>
    public class TwitterSettingsStep : IRecipeStepHandler
    {
        private readonly ITwitterSettingsService _twitterService;
        private readonly TwitterSettings _twitterSettings;

        public TwitterSettingsStep(ITwitterSettingsService twitterService, TwitterSettings twitterSettings)
        {
            _twitterService = twitterService;
            _twitterSettings = twitterSettings;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, nameof(TwitterSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<TwitterSettingsStepModel>();

            _twitterSettings.ConsumerKey = model.ConsumerKey;
            _twitterSettings.ConsumerSecret = model.ConsumerSecret;
            _twitterSettings.AccessToken = model.AccessToken;
            _twitterSettings.AccessTokenSecret = model.AccessTokenSecret;

            await _twitterService.UpdateSettingsAsync(_twitterSettings);
        }
    }

    public class TwitterSettingsStepModel
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
    }
}
