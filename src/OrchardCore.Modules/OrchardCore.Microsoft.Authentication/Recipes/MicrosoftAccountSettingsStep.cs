using System;
using System.Threading.Tasks;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Microsoft.Authentication.Recipes
{
    /// <summary>
    /// This recipe step sets Microsoft Account settings.
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
            if (!String.Equals(context.Name, nameof(MicrosoftAccountSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<MicrosoftAccountSettingsStepModel>();
            var settings = await _microsoftAccountService.LoadSettingsAsync();

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
