using OrchardCore.Https.Settings;

namespace OrchardCore.Https.ViewModels;

public class HttpsSettingsViewModel
{
    public bool IsHttpsRequest { get; set; }
    public HttpStrictTransportSecurityMode StrictTransportSecurityMode { get; set; }
    public bool RequireHttps { get; set; }
    public bool RequireHttpsPermanent { get; set; }
    public int? SslPort { get; set; }
}
