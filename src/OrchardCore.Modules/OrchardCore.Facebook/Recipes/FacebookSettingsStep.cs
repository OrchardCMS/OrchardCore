using System;
using System.Threading.Tasks;
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

        public FacebookSettingsStep(IFacebookService facebookService)
        {
            _facebookService = facebookService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "FacebookCoreSettings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<FacebookCoreSettingsStepModel>();
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
