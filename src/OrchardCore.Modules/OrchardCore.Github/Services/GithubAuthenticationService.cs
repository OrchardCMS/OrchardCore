using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Github.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Github.Services
{
    public class GithubAuthenticationService : IGithubAuthenticationService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<GithubAuthenticationService> T;
        private readonly ShellSettings _shellSettings;

        public GithubAuthenticationService(
            ISiteService siteService,
            ShellSettings shellSettings,
            IStringLocalizer<GithubAuthenticationService> stringLocalizer)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<GithubAuthenticationSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<GithubAuthenticationSettings>();
        }

        public async Task UpdateSettingsAsync(GithubAuthenticationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            var container = await _siteService.GetSiteSettingsAsync();
            container.Alter<GithubAuthenticationSettings>(nameof(GithubAuthenticationSettings), aspect =>
            {
                aspect.ClientID = settings.ClientID;
                aspect.ClientSecret = settings.ClientSecret;
                aspect.CallbackPath = settings.CallbackPath;
            });
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public IEnumerable<ValidationResult> ValidateSettings(GithubAuthenticationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(settings.ClientID))
            {
                yield return new ValidationResult(T["ClientID is required"], new string[] { nameof(settings.ClientID) });
            }

            if (string.IsNullOrWhiteSpace(settings.ClientSecret))
            {
                yield return new ValidationResult(T["ClientSecret is required"], new string[] { nameof(settings.ClientSecret) });
            }
        }

    }
}
