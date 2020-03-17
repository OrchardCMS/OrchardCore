using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace OrchardCore.ReverseProxy.Services
{
    public class ForwardedHeadersOptionsConfiguration : IConfigureOptions<ForwardedHeadersOptions>
    {
        private readonly ReverseProxyService _reverseProxyService;

        public ForwardedHeadersOptionsConfiguration(ReverseProxyService reverseProxyService)
        {
            _reverseProxyService = reverseProxyService;
        }

        public void Configure(ForwardedHeadersOptions options)
        {
            var reverseProxySettings = _reverseProxyService.GetSettingsAsync().GetAwaiter().GetResult();
            options.ForwardedHeaders = reverseProxySettings.ForwardedHeaders;

            // later we can add known networks and know proxies, for now we accept all
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        }
    }
}
