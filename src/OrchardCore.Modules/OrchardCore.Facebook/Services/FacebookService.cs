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
            ArgumentNullException.ThrowIfNull(settings);

            var results = new List<ValidationResult>();

            if (string.IsNullOrEmpty(settings.AppId))
            {
                results.Add(new ValidationResult(S["The AppId is required."],
                [
                    nameof(settings.AppId),
                ]));
            }

            if (string.IsNullOrEmpty(settings.AppSecret))
            {
                results.Add(new ValidationResult(S["The App Secret is required."],
                [
                    nameof(settings.AppSecret),
                ]));
            }

            return results;
        }
    }
}
