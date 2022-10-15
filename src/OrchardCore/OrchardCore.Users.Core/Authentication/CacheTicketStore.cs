using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Users.Authentication;

public class CacheTicketStore : ITicketStore
{
    private const string _keyPrefix = "ocauth-ticket";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private IDataProtector _dataProtector;

    public CacheTicketStore(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task RemoveAsync(string key)
    {
        var cacheKey = String.Concat(_keyPrefix, "-", key);
        var cache = _httpContextAccessor.HttpContext.RequestServices.GetService<IDistributedCache>();
        await cache.RemoveAsync(cacheKey);
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var cacheKey = String.Concat(_keyPrefix, "-", key);
        var cache = _httpContextAccessor.HttpContext.RequestServices.GetService<IDistributedCache>();
        _dataProtector ??= _httpContextAccessor.HttpContext.RequestServices.GetService<IDataProtectionProvider>()
                                .CreateProtector($"{nameof(CacheTicketStore)}_{IdentityConstants.ApplicationScheme}");

        var userId = string.Empty;
        var userName = string.Empty;
        if (ticket.AuthenticationScheme == IdentityConstants.ApplicationScheme)
        {
            userId = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            userName = ticket.Principal.FindFirst(ClaimTypes.Name)?.Value ?? ticket.Principal.FindFirst("name")?.Value;
        }

        var protectedBytes = _dataProtector.Protect(SerializeTicket(ticket));
        var data = new TicketRecord
        {
            Key = key,
            UserId = userId,
            Username = userName,
            LastActivity = DateTime.UtcNow,
            Value = protectedBytes,
            Expires = ticket.Properties.ExpiresUtc.Value.UtcDateTime,
        };
        var bytes = Serialize(data);
        await cache.SetAsync(cacheKey, bytes, new DistributedCacheEntryOptions() { AbsoluteExpiration = ticket.Properties.ExpiresUtc.Value });
    }
    public async Task<AuthenticationTicket> RetrieveAsync(string key)
    {
        var cacheKey = String.Concat(_keyPrefix, "-", key);
        var cache = _httpContextAccessor.HttpContext.RequestServices.GetService<IDistributedCache>();
        var bytes = await cache.GetAsync(cacheKey);
        if (bytes == null || bytes.Length == 0)
            return null;

        var data = Deserialize(bytes);
        _dataProtector ??= _httpContextAccessor.HttpContext.RequestServices.GetService<IDataProtectionProvider>()
                                .CreateProtector($"{nameof(CacheTicketStore)}_{IdentityConstants.ApplicationScheme}");
        var ticket = DeserializeTicket(_dataProtector.Unprotect(data.Value));

        return ticket;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString();
        var cacheKey = String.Concat(_keyPrefix, "-", key);

        var userId = string.Empty;
        var userName = string.Empty;

        if (ticket.AuthenticationScheme == IdentityConstants.ApplicationScheme)
        {
            userId = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value; ;
            userName = ticket.Principal.FindFirst(ClaimTypes.Name)?.Value ?? ticket.Principal.FindFirst("name")?.Value;
        }

        _dataProtector ??= _httpContextAccessor.HttpContext.RequestServices.GetService<IDataProtectionProvider>()
                                .CreateProtector($"{nameof(CacheTicketStore)}_{IdentityConstants.ApplicationScheme}");

        var protectedBytes = _dataProtector.Protect(SerializeTicket(ticket));
        var data = new TicketRecord
        {
            Key = key,
            UserId = userId,
            Username = userName,
            LastActivity = DateTime.UtcNow,
            Value = protectedBytes,
            Expires = ticket.Properties.ExpiresUtc.Value.UtcDateTime,
        };

        var cache = _httpContextAccessor.HttpContext.RequestServices.GetService<IDistributedCache>();
        var ticketBytes = Serialize(data);
        await cache.SetAsync(cacheKey, ticketBytes, new DistributedCacheEntryOptions() { AbsoluteExpiration = ticket.Properties.ExpiresUtc.Value });

        return key;
    }

    private byte[] SerializeTicket(AuthenticationTicket source)
        => TicketSerializer.Default.Serialize(source);

    private AuthenticationTicket DeserializeTicket(byte[] source)
        => source == null ? null : TicketSerializer.Default.Deserialize(source);

    private byte[] Serialize(TicketRecord m)
    {
        using var ms = new MemoryStream();

        using var writer = new BinaryWriter(ms);
        writer.Write(m.Key);
        writer.Write(m.UserId);
        writer.Write(m.Username);
        writer.Write7BitEncodedInt64(m.LastActivity.Ticks);
        writer.Write7BitEncodedInt(m.Value.Length);
        writer.Write(m.Value);
        writer.Write7BitEncodedInt64(m.Expires.Value.Ticks);

        return ms.ToArray();
    }

    private TicketRecord Deserialize(byte[] byteArray)
    {
        using var ms = new MemoryStream(byteArray);
        using var reader = new BinaryReader(ms);
        var ticket = new TicketRecord();
        ticket.Key = reader.ReadString();
        ticket.UserId = reader.ReadString();
        ticket.Username = reader.ReadString();
        var activityTicks = reader.Read7BitEncodedInt64();
        ticket.LastActivity = new DateTime(activityTicks, DateTimeKind.Utc);
        var bytesLength = reader.Read7BitEncodedInt();
        ticket.Value = reader.ReadBytes(bytesLength);
        var expireTicks = reader.Read7BitEncodedInt64();
        ticket.Expires = new DateTime(expireTicks, DateTimeKind.Utc);

        return ticket;        
    }
}


public record class TicketRecord
{
    public string Key { get; set; }
    public string UserId { get; set; }
    public string Username { get; set; }
    public DateTime LastActivity { get; set; }
    public byte[] Value { get; set; }
    public DateTime? Expires { get; set; }
}
