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
