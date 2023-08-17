using System;
using System.Threading.Tasks;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes
{
    /// <summary>
    /// This recipe step sets general OpenID Connect Client settings.
    /// </summary>
    public class OpenIdClientSettingsStep : IRecipeStepHandler
    {
        private readonly IOpenIdClientService _clientService;

        public OpenIdClientSettingsStep(IOpenIdClientService clientService)
        {
            _clientService = clientService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "OpenIdClientSettings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<OpenIdClientSettingsStepModel>();
            var settings = await _clientService.LoadSettingsAsync();

            settings.Scopes = model.Scopes.Split(' ', ',');
            settings.Authority = !string.IsNullOrEmpty(model.Authority) ? new Uri(model.Authority, UriKind.Absolute) : null;
            settings.CallbackPath = model.CallbackPath;
            settings.ClientId = model.ClientId;
            settings.ClientSecret = model.ClientSecret;
            settings.DisplayName = model.DisplayName;
            settings.ResponseMode = model.ResponseMode;
            settings.ResponseType = model.ResponseType;
            settings.SignedOutCallbackPath = model.SignedOutCallbackPath;
            settings.SignedOutRedirectUri = model.SignedOutRedirectUri;
            settings.StoreExternalTokens = model.StoreExternalTokens;
            settings.Parameters = model.Parameters;

            await _clientService.UpdateSettingsAsync(settings);
        }
    }

    public class OpenIdClientSettingsStepModel
    {
        public string DisplayName { get; set; }

        [Url]
        public string Authority { get; set; }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackPath { get; set; }
        public string SignedOutRedirectUri { get; set; }
        public string SignedOutCallbackPath { get; set; }
        public string Scopes { get; set; }
        public string ResponseType { get; set; }
        public string ResponseMode { get; set; }
        public bool StoreExternalTokens { get; set; }
        public ParameterSetting[] Parameters { get; set; }
    }
}
