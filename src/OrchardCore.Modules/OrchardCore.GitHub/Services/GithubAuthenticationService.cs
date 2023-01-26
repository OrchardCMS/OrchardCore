using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.GitHub.Settings;
using OrchardCore.Security.Services;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Services
{
    public class GitHubAuthenticationService : OAuthSettingsService<GitHubAuthenticationSettings>
    {
        public GitHubAuthenticationService(
            ISiteService siteService,
            IStringLocalizer<OAuthSettingsService<GitHubAuthenticationSettings>> stringLocalizer) : base(siteService, stringLocalizer)
        {
        }

        public override IEnumerable<ValidationResult> ValidateSettings(GitHubAuthenticationSettings settings)
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
