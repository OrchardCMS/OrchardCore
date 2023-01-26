using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Security.Services;
using OrchardCore.Settings;

namespace OrchardCore.Google.Authentication.Services
{
    public class GoogleAuthenticationService : SocialAuthenticationService<GoogleAuthenticationSettings>
    {
        public GoogleAuthenticationService(
            ISiteService siteService,
            IStringLocalizer<SocialAuthenticationService<GoogleAuthenticationSettings>> stringLocalizer) : base(siteService, stringLocalizer)
        {
        }

        public override IEnumerable<ValidationResult> ValidateSettings(GoogleAuthenticationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var results = new List<ValidationResult>();

            if (String.IsNullOrEmpty(settings.ClientID))
            {
                results.Add(new ValidationResult(S["The Client ID is required."], new[]
                {
                    nameof(settings.ClientID)
                }));
            }

            if (String.IsNullOrEmpty(settings.ClientSecret))
            {
                results.Add(new ValidationResult(S["The Client Secret is required."], new[]
                {
                    nameof(settings.ClientSecret)
                }));
            }

            return results;
        }
    }
}
