using Microsoft.Extensions.Options;
using OrchardCore.Https.Settings;

namespace OrchardCore.Https.Services;

/// <summary>
/// Configures the <see cref="HttpsSettings"/> options from the tenant site settings.
/// </summary>
internal sealed class HttpsSettingsConfiguration : IConfigureOptions<HttpsSettings>
{
    private readonly IHttpsService _httpsService;

    public HttpsSettingsConfiguration(IHttpsService httpsService)
    {
        _httpsService = httpsService;
    }

    public void Configure(HttpsSettings options)
    {
        var settings = _httpsService.GetSettingsAsync().GetAwaiter().GetResult();

        options.StrictTransportSecurityMode = settings.StrictTransportSecurityMode;
        options.RequireHttps = settings.RequireHttps;
        options.RequireHttpsPermanent = settings.RequireHttpsPermanent;
        options.SslPort = settings.SslPort;
    }
}
