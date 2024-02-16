using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public TwitterSettingsStep(
            ITwitterSettingsService twitterService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _twitterService = twitterService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(TwitterSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<TwitterSettingsStepModel>(_jsonSerializerOptions);
            var settings = await _twitterService.LoadSettingsAsync();

            settings.ConsumerKey = model.ConsumerKey;
            settings.ConsumerSecret = model.ConsumerSecret;
            settings.AccessToken = model.AccessToken;
            settings.AccessTokenSecret = model.AccessTokenSecret;

            await _twitterService.UpdateSettingsAsync(settings);
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
