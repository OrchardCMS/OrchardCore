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
    /// This recipe step sets general OpenID Connect Client settings.
    /// </summary>
    public class AzureADSettingsStep : IRecipeStepHandler
    {
        private readonly IAzureADService _azureADService;
        private readonly AzureADSettings _azureADSettings;

        public AzureADSettingsStep(IAzureADService azureADService, IOptions<AzureADSettings> azureADSettings)
        {
            _azureADService = azureADService;
            _azureADSettings = azureADSettings.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, nameof(AzureADSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<AzureADSettingsStepModel>();

            _azureADSettings.AppId = model.AppId;
            _azureADSettings.TenantId = model.TenantId;
            _azureADSettings.DisplayName = model.DisplayName;
            _azureADSettings.CallbackPath = model.CallbackPath;

            await _azureADService.UpdateSettingsAsync(_azureADSettings);
        }
    }

    public class AzureADSettingsStepModel
    {
        public string DisplayName { get; set; }
        public string AppId { get; set; }
        public string TenantId { get; set; }
        public string CallbackPath { get; set; }
    }
}
