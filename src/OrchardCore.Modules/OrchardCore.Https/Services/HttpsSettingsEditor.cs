using Microsoft.Extensions.Localization;
using OrchardCore.Https.Settings;

namespace OrchardCore.Https.Services;

internal static class HttpsSettingsEditor
{
    internal static Dictionary<string, string[]> Validate(HttpsSettings settings, IStringLocalizer localizer)
    {
        var errors = new Dictionary<string, string[]>();
        if (!Enum.IsDefined(settings.StrictTransportSecurityMode))
        {
            errors[nameof(settings.StrictTransportSecurityMode)] = [localizer["Select a valid HSTS mode."]];
        }
        if (settings.SslPort is < 1 or > 65535)
        {
            errors[nameof(settings.SslPort)] = [localizer["The HTTPS port must be between 1 and 65535, or empty for automatic detection."]];
        }
        return errors;
    }

    internal static bool Apply(HttpsSettings settings, HttpsSettings proposed)
    {
        if (settings.StrictTransportSecurityMode == proposed.StrictTransportSecurityMode
            && settings.RequireHttps == proposed.RequireHttps
            && settings.RequireHttpsPermanent == proposed.RequireHttpsPermanent
            && settings.SslPort == proposed.SslPort)
        {
            return false;
        }
        settings.StrictTransportSecurityMode = proposed.StrictTransportSecurityMode;
        settings.RequireHttps = proposed.RequireHttps;
        settings.RequireHttpsPermanent = proposed.RequireHttpsPermanent;
        settings.SslPort = proposed.SslPort;
        return true;
    }
}
