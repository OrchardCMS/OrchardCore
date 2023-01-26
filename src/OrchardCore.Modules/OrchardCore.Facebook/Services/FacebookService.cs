using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Facebook.Settings;
using OrchardCore.Security.Services;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Services
{
    public class FacebookService : OAuthSettingsService<FacebookSettings>
    {
        public FacebookService(
            ISiteService siteService,
            IStringLocalizer<OAuthSettingsService<FacebookSettings>> stringLocalizer) : base(siteService, stringLocalizer)
        {
        }

        public override IEnumerable<ValidationResult> ValidateSettings(FacebookSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var results = new List<ValidationResult>();

            if (String.IsNullOrEmpty(settings.AppId))
            {
                results.Add(new ValidationResult(S["The AppId is required."], new[]
                {
                    nameof(settings.AppId)
                }));
            }

            if (String.IsNullOrEmpty(settings.AppSecret))
            {
                results.Add(new ValidationResult(S["The App Secret is required."], new[]
                {
                    nameof(settings.AppSecret)
                }));
            }

            return results;
        }
    }
}
