using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.GitHub.Settings;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Services
{
    public class GitHubAuthenticationService : IGitHubAuthenticationService
    {
        private readonly ISiteService _siteService;
        protected readonly IStringLocalizer S;

        public GitHubAuthenticationService(
            ISiteService siteService,
            IStringLocalizer<GitHubAuthenticationService> stringLocalizer)
        {
            _siteService = siteService;
            S = stringLocalizer;
        }

        public async Task<GitHubAuthenticationSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<GitHubAuthenticationSettings>();
        }

        public async Task<GitHubAuthenticationSettings> LoadSettingsAsync()
        {
            var container = await _siteService.LoadSiteSettingsAsync();
            return container.As<GitHubAuthenticationSettings>();
        }

        public async Task UpdateSettingsAsync(GitHubAuthenticationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var container = await _siteService.LoadSiteSettingsAsync();
            container.Alter<GitHubAuthenticationSettings>(nameof(GitHubAuthenticationSettings), aspect =>
            {
                aspect.ClientID = settings.ClientID;
                aspect.ClientSecret = settings.ClientSecret;
                aspect.CallbackPath = settings.CallbackPath;
            });

            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public IEnumerable<ValidationResult> ValidateSettings(GitHubAuthenticationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (String.IsNullOrWhiteSpace(settings.ClientID))
            {
                yield return new ValidationResult(S["ClientID is required"], new string[] { nameof(settings.ClientID) });
            }

            if (String.IsNullOrWhiteSpace(settings.ClientSecret))
            {
                yield return new ValidationResult(S["ClientSecret is required"], new string[] { nameof(settings.ClientSecret) });
            }
        }
    }
}
