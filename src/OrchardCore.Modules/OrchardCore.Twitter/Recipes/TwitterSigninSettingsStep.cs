using System;
using System.Threading.Tasks;
using OrchardCore.Twitter.Services;
using OrchardCore.Twitter.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Twitter.Recipes
{
    /// <summary>
    /// This recipe step sets Microsoft Account settings.
    /// </summary>
    public class TwitterSigninSettingsStep : IRecipeStepHandler
    {
        private readonly ITwitterSigninService _twitterLoginService;

        public TwitterSigninSettingsStep(ITwitterSigninService twitterLoginService)
        {
            _twitterLoginService = twitterLoginService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(TwitterSigninSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var model = context.Step.ToObject<TwitterLoginSettingsStepModel>();
            var settings = await _twitterLoginService.GetSettingsAsync();
            settings.ConsumerKey = model.ConsumerKey;
            settings.ConsumerSecret = model.ConsumerSecret;
            settings.CallbackPath = model.CallbackPath;
            await _twitterLoginService.UpdateSettingsAsync(settings);
        }
    }

    public class TwitterLoginSettingsStepModel
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string CallbackPath { get; set; }
    }
}