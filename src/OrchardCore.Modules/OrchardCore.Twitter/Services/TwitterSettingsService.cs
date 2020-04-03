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
using OrchardCore.Twitter.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Twitter.Services
{
    public class TwitterSettingsService : ITwitterSettingsService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<TwitterSettingsService> T;
        private readonly ShellSettings _shellSettings;

        public TwitterSettingsService(
            ISiteService siteService,
            ShellSettings shellSettings,
            IStringLocalizer<TwitterSettingsService> stringLocalizer)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<TwitterSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<TwitterSettings>();
        }

        public async Task UpdateSettingsAsync(TwitterSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            var container = await _siteService.GetSiteSettingsAsync();
            container.Alter<TwitterSettings>(nameof(TwitterSettings), aspect =>
            {
                aspect.ConsumerKey = settings.ConsumerKey;
                aspect.ConsumerSecret = settings.ConsumerSecret;
                aspect.AccessToken = settings.AccessToken;
                aspect.AccessTokenSecret = settings.AccessTokenSecret;
            });
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public IEnumerable<ValidationResult> ValidateSettings(TwitterSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(settings.ConsumerKey))
            {
                yield return new ValidationResult(T["ConsumerKey is required"], new string[] { nameof(settings.ConsumerKey) });
            }

            if (string.IsNullOrWhiteSpace(settings.ConsumerSecret))
            {
                yield return new ValidationResult(T["ConsumerSecret is required"], new string[] { nameof(settings.ConsumerSecret) });
            }

            if (string.IsNullOrWhiteSpace(settings.AccessToken))
            {
                yield return new ValidationResult(T["Access Token is required"], new string[] { nameof(settings.AccessToken) });
            }

            if (string.IsNullOrWhiteSpace(settings.AccessTokenSecret))
            {
                yield return new ValidationResult(T["Access Token Secret is required"], new string[] { nameof(settings.AccessTokenSecret) });
            }
        }

    }
}
