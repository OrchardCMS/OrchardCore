using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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
        private readonly MicrosoftAccountSettings _microsoftAccountSettings;

        public MicrosoftAccountSettingsStep(
            IMicrosoftAccountService microsoftAccountService,
            IOptions<MicrosoftAccountSettings> microsoftAccountSettings)
        {
            _microsoftAccountService = microsoftAccountService;
            _microsoftAccountSettings = microsoftAccountSettings.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, nameof(MicrosoftAccountSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<MicrosoftAccountSettingsStepModel>();

            _microsoftAccountSettings.AppId = model.AppId;
            _microsoftAccountSettings.AppSecret = model.AppSecret;
            _microsoftAccountSettings.CallbackPath = model.CallbackPath;

            await _microsoftAccountService.UpdateSettingsAsync(_microsoftAccountSettings);
        }
    }

    public class MicrosoftAccountSettingsStepModel
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string CallbackPath { get; set; }
    }
}
