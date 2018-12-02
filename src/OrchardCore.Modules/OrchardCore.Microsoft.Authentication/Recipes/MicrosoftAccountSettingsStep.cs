using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
//using static OrchardCore.Microsoft.Authentication.Settings.AzureADAuthenticationSettings;

namespace OrchardCore.Microsoft.Authentication.Recipes
{
    /// <summary>
    /// This recipe step sets general OpenID Connect Client settings.
    /// </summary>
    public class MicrosoftAccountSettingsStep : IRecipeStepHandler
    {
        private readonly IMicrosoftAccountService _microsoftAccountService;

        public MicrosoftAccountSettingsStep(IMicrosoftAccountService microsoftAccountService)
        {
            _microsoftAccountService = microsoftAccountService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(MicrosoftAccountSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<MicrosoftAccountSettingsStepModel>();

            var settings = await _microsoftAccountService.GetSettingsAsync();
            settings.AppId = model.AppId;
            settings.AppSecret = model.AppSecret;
            settings.CallbackPath = model.CallbackPath;
            await _microsoftAccountService.UpdateSettingsAsync(settings);
        }
    }

    public class MicrosoftAccountSettingsStepModel
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string CallbackPath { get; set; }
    }
}