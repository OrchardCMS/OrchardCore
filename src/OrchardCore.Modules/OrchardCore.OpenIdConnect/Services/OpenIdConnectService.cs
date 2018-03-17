using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.OpenIdConnect.Services
{
    public class OpenIdConnectService : IOpenIdConnectService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<OpenIdConnectService> T;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ShellSettings _shellSettings;

        public OpenIdConnectService(ISiteService siteService,
            ShellSettings shellSettings,
            IStringLocalizer<OpenIdConnectService> stringLocalizer,
            IDataProtectionProvider dataProtectionProvider)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            T = stringLocalizer;
            _dataProtectionProvider = dataProtectionProvider;
        }

        public async Task<OpenIdConnectSettings> GetOpenIdConnectSettings()
        {
            var settings = await _siteService.GetSiteSettingsAsync();
            var result = settings.As<OpenIdConnectSettings>();
            if (result == null)
            {
                result = new OpenIdConnectSettings();
            }
            return result;
        }

        public bool IsValidOpenIdConnectSettings(OpenIdConnectSettings settings, ModelStateDictionary modelState)
        {
            if (settings == null)
            {
                modelState.AddModelError("", T["Settings are not stablished."]);
                return false;
            }

            ValidateUrisSchema(new string[] { settings.Authority }, !settings.TestingModeEnabled, modelState, "Authority");

            return modelState.IsValid;
        }

        public bool IsValidOpenIdConnectSettings(OpenIdConnectSettings settings)
        {
            var modelState = new ModelStateDictionary();
            return IsValidOpenIdConnectSettings(settings, modelState);
        }

        public string Protect(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;
            var protector = _dataProtectionProvider.CreateProtector(nameof(OpenIdConnectSettings), _shellSettings.Name);
            return protector.Protect(value);
        }

        public string Unprotect(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;
            var protector = _dataProtectionProvider.CreateProtector(nameof(OpenIdConnectSettings), _shellSettings.Name);
            return protector.Unprotect(value);
        }

        public async Task UpdateOpenIdConnectSettingsAsync(OpenIdConnectSettings settings)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            siteSettings.Properties[nameof(OpenIdConnectSettings)] = JObject.FromObject(settings);
            await _siteService.UpdateSiteSettingsAsync(siteSettings);
        }

        private void ValidateUrisSchema(IEnumerable<string> uriStrings, bool onlyAllowHttps, ModelStateDictionary modelState, string modelStateKey)
        {
            if (uriStrings == null)
            {
                modelState.AddModelError(modelStateKey, T["Invalid url."]);
                return;
            }
            foreach (var uriString in uriStrings.Select(a => (a ?? "").Trim()))
            {
                Uri uri;
                if (!Uri.TryCreate(uriString, UriKind.Absolute, out uri) || ((onlyAllowHttps || uri.Scheme != "http") && uri.Scheme != "https"))
                {
                    var message = onlyAllowHttps ? T["Invalid url. Non https urls are only allowed in testing mode."] : T["Invalid url."];
                    modelState.AddModelError(modelStateKey, T[message]);
                }
            }
        }

    }
}
