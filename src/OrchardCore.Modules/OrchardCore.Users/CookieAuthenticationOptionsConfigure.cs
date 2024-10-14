using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Users.Authentication;

public sealed class CookieAuthenticationOptionsConfigure : IConfigureNamedOptions<CookieAuthenticationOptions>
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
