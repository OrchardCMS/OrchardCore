using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Security.Services;
using OrchardCore.Settings;

namespace OrchardCore.Google.Authentication.Services;

public class GoogleAuthenticationService : OAuthSettingsService<GoogleAuthenticationSettings>, IGoogleAuthenticationService
{
    public GoogleAuthenticationService(
        ISiteService siteService,
        IStringLocalizer<OAuthSettingsService<GoogleAuthenticationSettings>> stringLocalizer) : base(siteService, stringLocalizer)
    {
    }

    public override IEnumerable<ValidationResult> ValidateSettings(GoogleAuthenticationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var results = new List<ValidationResult>();

        if (string.IsNullOrEmpty(settings.ClientID))
        {
            results.Add(new ValidationResult(S["The Client ID is required."],
            [
                nameof(settings.ClientID)
            ]));
        }

        if (string.IsNullOrEmpty(settings.ClientSecret))
        {
            results.Add(new ValidationResult(S["The Client Secret is required."],
            [
                nameof(settings.ClientSecret)
            ]));
        }

        return results;
    }
}
