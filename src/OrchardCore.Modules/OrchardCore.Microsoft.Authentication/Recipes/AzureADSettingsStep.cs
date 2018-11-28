using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Microsoft.Authentication.Recipes
{
    /// <summary>
    /// This recipe step sets general OpenID Connect Client settings.
    /// </summary>
    public class AzureADSettingsStep : IRecipeStepHandler
    {
        private readonly IAzureADService _loginService;

        public AzureADSettingsStep(IAzureADService loginService)
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