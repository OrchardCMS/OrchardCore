using System.Text.Json.Nodes;
using OrchardCore.Data.Migration;
using OrchardCore.Https.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Https.Migrations;

internal sealed class HttpsSettingsMigrations : DataMigration
{
    private readonly ISiteService _siteService;

    public HttpsSettingsMigrations(ISiteService siteService)
    {
        _siteService = siteService;
    }

    [Obsolete]
    public async Task<int> CreateAsync()
    {
        var site = await _siteService.LoadSiteSettingsAsync();

        if (site.Properties[nameof(HttpsSettings)] is not JsonObject settingsObject)
        {
            return 1;
        }

        var requiresUpdate = false;

        if (!settingsObject.ContainsKey(nameof(HttpsSettings.StrictTransportSecurityMode)) &&
            settingsObject[nameof(HttpsSettings.EnableStrictTransportSecurity)] is JsonValue strictTransportSecurityValue &&
            strictTransportSecurityValue.TryGetValue<bool>(out var enableStrictTransportSecurity))
        {
            settingsObject[nameof(HttpsSettings.StrictTransportSecurityMode)] = enableStrictTransportSecurity
                ? HttpStrictTransportSecurityMode.Enabled.ToString()
                : HttpStrictTransportSecurityMode.Disabled.ToString();

            requiresUpdate = true;
        }

        if (settingsObject.Remove(nameof(HttpsSettings.EnableStrictTransportSecurity)))
        {
            requiresUpdate = true;
        }

        if (requiresUpdate)
        {
            await _siteService.UpdateSiteSettingsAsync(site);
        }

        return 1;
    }
}
