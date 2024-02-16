using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Facebook.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Facebook.Recipes
{
    /// <summary>
    /// This recipe step sets general Facebook Login settings.
    /// </summary>
    public class FacebookSettingsStep : IRecipeStepHandler
    {
        private readonly IFacebookService _facebookService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public FacebookSettingsStep(
            IFacebookService facebookService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _facebookService = facebookService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "FacebookCoreSettings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<FacebookCoreSettingsStepModel>(_jsonSerializerOptions);
            var settings = await _facebookService.GetSettingsAsync();

            settings.AppId = model.AppId;
            settings.AppSecret = model.AppSecret;
            settings.SdkJs = model.SdkJs ?? "sdk.js";
            settings.FBInit = model.FBInit;
            settings.FBInitParams = model.FBInitParams;
            settings.Version = model.Version ?? "3.2";

            await _facebookService.UpdateSettingsAsync(settings);
        }
    }

    public class FacebookCoreSettingsStepModel
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string SdkJs { get; set; }
        public bool FBInit { get; set; }
        public string FBInitParams { get; set; }
        public string Version { get; set; }
    }
}
