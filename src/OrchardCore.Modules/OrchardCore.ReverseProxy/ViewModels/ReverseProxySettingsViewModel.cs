using OrchardCore.ReverseProxy.Settings;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.ReverseProxy.ViewModels;

public class ReverseProxySettingsViewModel : ConfigurableSettingsViewModel<ReverseProxySettings>
{
    public bool EnableXForwardedFor { get; set; }

    public bool EnableXForwardedProto { get; set; }

    public bool EnableXForwardedHost { get; set; }

    public string KnownNetworks { get; set; }

    public string KnownProxies { get; set; }
}
