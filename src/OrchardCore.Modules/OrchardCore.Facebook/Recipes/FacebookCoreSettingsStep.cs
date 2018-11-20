using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using OrchardCore.Facebook.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Facebook.Recipes
{
    /// <summary>
    /// This recipe step sets general OpenID Connect Client settings.
    /// </summary>
    public class FacebookCoreSettingsStep : IRecipeStepHandler
    {
        private readonly IFacebookCoreService _loginService;

        public FacebookCoreSettingsStep(IFacebookCoreService loginService)
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
            settings.AppSecret= model.AppSecret;

            await _loginService.UpdateSettingsAsync(settings);
        }
    }

    public class FacebookCoreSettingsStepModel
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }
}