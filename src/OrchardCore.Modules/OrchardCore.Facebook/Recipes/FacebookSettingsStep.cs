using System;
using System.Threading.Tasks;
using OrchardCore.Facebook.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Facebook.Recipes
{
    /// <summary>
    /// This recipe step sets general OpenID Connect Client settings.
    /// </summary>
    public class FacebookSettingsStep : IRecipeStepHandler
    {
        private readonly IFacebookService _loginService;

        public FacebookSettingsStep(IFacebookService loginService)
        {
            _loginService = loginService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "FacebookCoreSettings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<FacebookCoreSettingsStepModel>();

            var settings = await _loginService.GetSettingsAsync();
            settings.AppId = model.AppId;
            settings.AppSecret = model.AppSecret;
            settings.SdkJs = model.SdkJs ?? "sdk.js";
            settings.FBInit = model.FBInit;
            settings.FBInitParams = model.FBInitParams;
            settings.Version = model.Version ?? "3.2";

            await _loginService.UpdateSettingsAsync(settings);
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
