using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Users.Authentication
{
    [Feature("OrchardCore.Users.Authentication.CacheTicketStore")]
    public class CacheTicketStoreStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureOptions<CookieAuthenticationOptionsConfigure>();
        }
    }

    public class CookieAuthenticationOptionsConfigure : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<CacheTicketStore> _logger;

        public CookieAuthenticationOptionsConfigure(
            IDistributedCache distributedCache,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<CacheTicketStore> logger)
        {
            _distributedCache = distributedCache;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            if (name == IdentityConstants.ApplicationScheme)
            {
                options.SessionStore = new CacheTicketStore(_distributedCache, _dataProtectionProvider, _logger);
            }
        }

        public void Configure(CookieAuthenticationOptions options)
        {
        }
    }
}
