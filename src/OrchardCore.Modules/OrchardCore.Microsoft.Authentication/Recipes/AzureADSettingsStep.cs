using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Microsoft.Authentication.Recipes
{
    /// <summary>
    /// This recipe step sets general Microsoft Entra ID settings.
    /// </summary>
    public class AzureADSettingsStep : IRecipeStepHandler
    {
        private readonly IAzureADService _azureADService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AzureADSettingsStep(
            IAzureADService azureADService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _azureADService = azureADService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(AzureADSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<AzureADSettingsStepModel>(_jsonSerializerOptions);
            var settings = await _azureADService.LoadSettingsAsync();

            settings.AppId = model.AppId;
            settings.TenantId = model.TenantId;
            settings.DisplayName = model.DisplayName;
            settings.CallbackPath = model.CallbackPath;

            await _azureADService.UpdateSettingsAsync(settings);
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
