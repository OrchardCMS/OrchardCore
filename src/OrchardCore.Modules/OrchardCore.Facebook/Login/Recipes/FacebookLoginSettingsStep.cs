using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Facebook.Login.Services;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Facebook.Login.Recipes
{
    /// <summary>
    /// This recipe step sets general Facebook Login settings.
    /// </summary>
    public class FacebookLoginSettingsStep : IRecipeStepHandler
    {
        private readonly IFacebookLoginService _loginService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public FacebookLoginSettingsStep(
            IFacebookLoginService loginService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _loginService = loginService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(FacebookLoginSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<FacebookLoginSettingsStepModel>(_jsonSerializerOptions);
            var settings = await _loginService.LoadSettingsAsync();

            settings.CallbackPath = model.CallbackPath;

            await _loginService.UpdateSettingsAsync(settings);
        }
    }

    public class FacebookLoginSettingsStepModel
    {
        public string CallbackPath { get; set; }
    }
}
