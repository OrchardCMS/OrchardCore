using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services
{
    public class TwitterSettingsService : ITwitterSettingsService
    {
        private readonly ISiteService _siteService;
        protected readonly IStringLocalizer S;

        public TwitterSettingsService(
            ISiteService siteService,
            IStringLocalizer<TwitterSettingsService> stringLocalizer)
        {
            _siteService = siteService;
            S = stringLocalizer;
        }

        public async Task<TwitterSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<TwitterSettings>();
        }

        public async Task<TwitterSettings> LoadSettingsAsync()
        {
            var container = await _siteService.LoadSiteSettingsAsync();
            return container.As<TwitterSettings>();
        }

        public async Task UpdateSettingsAsync(TwitterSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var container = await _siteService.LoadSiteSettingsAsync();
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

            if (String.IsNullOrWhiteSpace(settings.ConsumerKey))
            {
                yield return new ValidationResult(S["ConsumerKey is required"], new string[] { nameof(settings.ConsumerKey) });
            }

            if (String.IsNullOrWhiteSpace(settings.ConsumerSecret))
            {
                yield return new ValidationResult(S["ConsumerSecret is required"], new string[] { nameof(settings.ConsumerSecret) });
            }

            if (String.IsNullOrWhiteSpace(settings.AccessToken))
            {
                yield return new ValidationResult(S["Access Token is required"], new string[] { nameof(settings.AccessToken) });
            }

            if (String.IsNullOrWhiteSpace(settings.AccessTokenSecret))
            {
                yield return new ValidationResult(S["Access Token Secret is required"], new string[] { nameof(settings.AccessTokenSecret) });
            }
        }
    }
}
