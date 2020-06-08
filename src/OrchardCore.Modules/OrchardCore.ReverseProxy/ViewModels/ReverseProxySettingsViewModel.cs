namespace OrchardCore.ReverseProxy.ViewModels
{
    public class ReverseProxySettingsViewModel
    {
        public bool EnableXForwardedFor { get; set; }

        public bool EnableXForwardedProto { get; set; }

        public bool EnableXForwardedHost { get; set; }
    }
}
