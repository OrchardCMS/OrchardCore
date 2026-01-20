#nullable enable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Users.Authentication;

public class CacheTicketStore : ITicketStore
{
    private const string KeyPrefix = "ocauth-ticket";

    private IDataProtector? _dataProtector;
    private readonly ILogger _logger;
    private readonly IDistributedCache _distributedCache;
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public CacheTicketStore(
        IDistributedCache distributedCache,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<CacheTicketStore> logger
    )
    {
        _distributedCache = distributedCache;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    private IDataProtector DataProtector => _dataProtector ??= _dataProtectionProvider
        .CreateProtector($"{nameof(CacheTicketStore)}_{IdentityConstants.ApplicationScheme}");

    public async Task RemoveAsync(string key)
    {
        var cacheKey = $"{KeyPrefix}-{key}";
        await _distributedCache.RemoveAsync(cacheKey);
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var cacheKey = $"{KeyPrefix}-{key}";

        try
        {
            var protectedBytes = DataProtector.Protect(SerializeTicket(ticket));
            await _distributedCache.SetAsync(cacheKey, protectedBytes,
                new DistributedCacheEntryOptions() { AbsoluteExpiration = ticket.Properties.ExpiresUtc });
        }
        catch (Exception e)
        {
            // Data Protection Error
            _logger.LogError(e, "{MethodName} failed  for '{Key}'.", nameof(RenewAsync), cacheKey);
        }
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var cacheKey = $"{KeyPrefix}-{key}";

        var bytes = await _distributedCache.GetAsync(cacheKey);
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
            _logger.LogError(e, "{MethodName} failed  for '{Key}'.", nameof(RetrieveAsync), cacheKey);
            return null;
        }
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString();
        var cacheKey = $"{KeyPrefix}-{key}";

        try
        {
            var protectedBytes = DataProtector.Protect(SerializeTicket(ticket));
            await _distributedCache.SetAsync(cacheKey, protectedBytes,
                new DistributedCacheEntryOptions() { AbsoluteExpiration = ticket.Properties.ExpiresUtc });

            return key;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{MethodName} failed  for '{Key}'.", nameof(StoreAsync), cacheKey);
            throw;
        }
    }

    private static byte[] SerializeTicket(AuthenticationTicket source)
        => TicketSerializer.Default.Serialize(source);

    private static AuthenticationTicket? DeserializeTicket(byte[]? source)
        => source == null ? null : TicketSerializer.Default.Deserialize(source);
}
