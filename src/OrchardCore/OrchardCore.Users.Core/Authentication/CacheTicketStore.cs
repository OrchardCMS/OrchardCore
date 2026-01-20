using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Users.Authentication;

public class CacheTicketStore : ITicketStore
{
    private const string KeyPrefix = "ocauth-ticket";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private IDataProtector _dataProtector;
    private ILogger _logger;

    public CacheTicketStore(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IDataProtector DataProtector => _dataProtector ??= _httpContextAccessor.HttpContext.RequestServices.GetService<IDataProtectionProvider>()
        .CreateProtector($"{nameof(CacheTicketStore)}_{IdentityConstants.ApplicationScheme}");

    public ILogger Logger => _logger ??= _httpContextAccessor.HttpContext.RequestServices.GetService<ILogger<CacheTicketStore>>();

    public async Task RemoveAsync(string key)
    {
        var cacheKey = $"{KeyPrefix}-{key}";
        var cache = _httpContextAccessor.HttpContext.RequestServices.GetService<IDistributedCache>();
        await cache.RemoveAsync(cacheKey);
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var cacheKey = $"{KeyPrefix}-{key}";
        var cache = _httpContextAccessor.HttpContext.RequestServices.GetService<IDistributedCache>();

        try
        {
            var protectedBytes = DataProtector.Protect(SerializeTicket(ticket));
            await cache.SetAsync(cacheKey, protectedBytes, new DistributedCacheEntryOptions() { AbsoluteExpiration = ticket.Properties.ExpiresUtc.Value });
        }
        catch (Exception e)
        {
            // Data Protection Error
            Logger.LogError(e, "{methodName} failed  for '{key}'.", nameof(RenewAsync), cacheKey);
        }
    }

    public async Task<AuthenticationTicket> RetrieveAsync(string key)
    {
        var cacheKey = $"{KeyPrefix}-{key}";
        var cache = _httpContextAccessor.HttpContext.RequestServices.GetService<IDistributedCache>();
        var bytes = await cache.GetAsync(cacheKey);
        if (bytes == null || bytes.Length == 0)
        {
            return null;
        }

        try
        {
            var ticket = DeserializeTicket(DataProtector.Unprotect(bytes));
            return ticket;
        }
        catch (Exception e)
        {
            // Data Protection Error
            Logger.LogError(e, "{methodName} failed  for '{key}'.", nameof(RetrieveAsync), cacheKey);
            return null;
        }
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString();
        var cacheKey = $"{KeyPrefix}-{key}";
        var cache = _httpContextAccessor.HttpContext.RequestServices.GetService<IDistributedCache>();

        try
        {
            var protectedBytes = DataProtector.Protect(SerializeTicket(ticket));
            await cache.SetAsync(cacheKey, protectedBytes, new DistributedCacheEntryOptions() { AbsoluteExpiration = ticket.Properties.ExpiresUtc.Value });
            return key;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "{methodName} failed  for '{key}'.", nameof(StoreAsync), cacheKey);
            return null;
        }
    }

    private static byte[] SerializeTicket(AuthenticationTicket source)
        => TicketSerializer.Default.Serialize(source);

    private static AuthenticationTicket DeserializeTicket(byte[] source)
        => source == null ? null : TicketSerializer.Default.Deserialize(source);
}
