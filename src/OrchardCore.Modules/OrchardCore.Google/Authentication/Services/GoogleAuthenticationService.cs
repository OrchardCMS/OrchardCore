using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Google.Authentication.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Google.Authentication.Services
{
    public class GoogleAuthenticationService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<GoogleAuthenticationService> T;
        private readonly ShellSettings _shellSettings;

        public GoogleAuthenticationService(
            ISiteService siteService,
            ShellSettings shellSettings,
            IStringLocalizer<GoogleAuthenticationService> stringLocalizer)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<GoogleAuthenticationSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<GoogleAuthenticationSettings>();
        }

        public async Task UpdateSettingsAsync(GoogleAuthenticationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            var container = await _siteService.GetSiteSettingsAsync();
            container.Alter<GoogleAuthenticationSettings>(nameof(GoogleAuthenticationSettings), aspect =>
            {
                aspect.ClientID = settings.ClientID;
                aspect.ClientSecret = settings.ClientSecret;
                aspect.CallbackPath = settings.CallbackPath;
            });
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public bool CheckSettings(GoogleAuthenticationSettings settings)
        {
            var obj = new GoogleAuthenticationSettingsViewModel()
            {
                ClientID = settings.ClientID,
                CallbackPath = settings.CallbackPath,
                ClientSecret = settings.ClientSecret
            };
            var vc = new ValidationContext(obj);
            return Validator.TryValidateObject(obj, vc, ImmutableArray.CreateBuilder<ValidationResult>());
        }
    }
}
