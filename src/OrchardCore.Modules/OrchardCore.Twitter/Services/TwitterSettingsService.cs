using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Security.Services;
using OrchardCore.Settings;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services
{
    public class TwitterSettingsService : OAuthSettingsService<TwitterSettings>
    {
        public TwitterSettingsService(
            ISiteService siteService,
            IStringLocalizer<OAuthSettingsService<TwitterSettings>> stringLocalizer) : base(siteService, stringLocalizer)
        {
        }

        public override IEnumerable<ValidationResult> ValidateSettings(TwitterSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            if (string.IsNullOrWhiteSpace(settings.ConsumerKey))
            {
                yield return new ValidationResult(S["ConsumerKey is required"], new string[] { nameof(settings.ConsumerKey) });
            }

            if (string.IsNullOrWhiteSpace(settings.ConsumerSecret))
            {
                yield return new ValidationResult(S["ConsumerSecret is required"], new string[] { nameof(settings.ConsumerSecret) });
            }

            if (string.IsNullOrWhiteSpace(settings.AccessToken))
            {
                yield return new ValidationResult(S["Access Token is required"], new string[] { nameof(settings.AccessToken) });
            }

            if (string.IsNullOrWhiteSpace(settings.AccessTokenSecret))
            {
                yield return new ValidationResult(S["Access Token Secret is required"], new string[] { nameof(settings.AccessTokenSecret) });
            }
        }
    }
}
