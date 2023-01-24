using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Facebook.Services;
using OrchardCore.Facebook.Settings;
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
        private readonly FacebookSettings _facebookSettings;

        public FacebookSettingsStep(IFacebookService facebookService, IOptions<FacebookSettings> facebookSettings)
        {
            _facebookService = facebookService;
            _facebookSettings = facebookSettings.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "FacebookCoreSettings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<FacebookCoreSettingsStepModel>();

            _facebookSettings.AppId = model.AppId;
            _facebookSettings.AppSecret = model.AppSecret;
            _facebookSettings.SdkJs = model.SdkJs ?? "sdk.js";
            _facebookSettings.FBInit = model.FBInit;
            _facebookSettings.FBInitParams = model.FBInitParams;
            _facebookSettings.Version = model.Version ?? "3.2";

            await _facebookService.UpdateSettingsAsync(_facebookSettings);
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
