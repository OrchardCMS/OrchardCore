using System;
using System.IO;
using System.Runtime.Serialization;
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
    private readonly IHttpContextAccessor _accessor;

    public CacheTicketStore(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public async Task RemoveAsync(string key)
    {
        var cache = _accessor.HttpContext.RequestServices.GetService<IDistributedCache>();
        var bytes = await cache.GetAsync($"{_keyPrefix}-{key}");
        if (bytes != null && bytes.Length > 0)
        {
            await cache.RemoveAsync($"{_keyPrefix}-{key}");
        }
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var cache = _accessor.HttpContext.RequestServices.GetService<IDistributedCache>();
        var dataProtector = _accessor.HttpContext.RequestServices.GetService<IDataProtectionProvider>()
                                .CreateProtector($"{nameof(CacheTicketStore)}_{IdentityConstants.ApplicationScheme}");
        var bytes = await cache.GetAsync($"{_keyPrefix}-{key}");
        if (bytes != null && bytes.Length > 0)
        {
            var ticketValue = dataProtector.Protect(SerializeTicket(ticket));
            var expiresUtc = ticket.Properties.ExpiresUtc;

            var data = Deserialize(bytes);
            // Update Last Activity Time
            data.LastActivityUtc = DateTime.UtcNow;
            data.ExpiresUtc = expiresUtc;
            data.Value = ticketValue;
            bytes = Serialize(data);
            await cache.SetAsync($"{_keyPrefix}-{key}", bytes, new DistributedCacheEntryOptions() { AbsoluteExpiration = expiresUtc.Value });
        }
    }
    public async Task<AuthenticationTicket> RetrieveAsync(string key)
    {
        var cache = _accessor.HttpContext.RequestServices.GetService<IDistributedCache>();
        var bytes = await cache.GetAsync($"{_keyPrefix}-{key}");

        if (bytes == null || bytes.Length == 0)
            return null;
        var data = Deserialize(bytes);
        // Update Last Activity Time
        data.LastActivityUtc = DateTime.UtcNow;
        bytes = Serialize(data);
        await cache.SetAsync($"{_keyPrefix}-{key}", bytes);

        var dataProtector = _accessor.HttpContext.RequestServices.GetService<IDataProtectionProvider>()
                                .CreateProtector($"{nameof(CacheTicketStore)}_{IdentityConstants.ApplicationScheme}");

        var ticket = DeserializeTicket(dataProtector.Unprotect(data.Value));
        return ticket;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString();

        var userId = string.Empty;
        var userName = string.Empty;

        var nameIdentifierClaim = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var nameClaim = ticket.Principal.FindFirst(ClaimTypes.Name)?.Value ?? ticket.Principal.FindFirst("name")?.Value;

        if (ticket.AuthenticationScheme == IdentityConstants.ApplicationScheme)
        {
            userId = nameIdentifierClaim;
            userName = nameClaim;
        }

        var dataProtector = _accessor.HttpContext.RequestServices.GetService<IDataProtectionProvider>()
                                .CreateProtector($"{nameof(CacheTicketStore)}_{IdentityConstants.ApplicationScheme}");

        var ticketValue = dataProtector.Protect(SerializeTicket(ticket));
        var expiresUtc = ticket.Properties.ExpiresUtc;

        var data = new TicketRecord
        {
            Key = key,
            UserId = userId,
            Username = userName,
            LastActivityUtc = DateTime.UtcNow,
            Value = ticketValue,
            ExpiresUtc = expiresUtc,
        };

        var cache = _accessor.HttpContext.RequestServices.GetService<IDistributedCache>();
        var bytes = Serialize(data);
        await cache.SetAsync($"{_keyPrefix}-{key}", bytes, new DistributedCacheEntryOptions() { AbsoluteExpiration = expiresUtc.Value });

        return key;
    }

    private byte[] SerializeTicket(AuthenticationTicket source)
        => TicketSerializer.Default.Serialize(source);

    private AuthenticationTicket DeserializeTicket(byte[] source)
        => source == null ? null : TicketSerializer.Default.Deserialize(source);

    private byte[] Serialize(TicketRecord m)
    {
        using (var ms = new MemoryStream())
        {
            (new DataContractSerializer(typeof(TicketRecord))).WriteObject(ms, m);
            return ms.ToArray();
        }
    }

    private TicketRecord Deserialize(byte[] byteArray)
    {
        using (var ms = new MemoryStream(byteArray))
        {
            return (TicketRecord)(new DataContractSerializer(typeof(TicketRecord))).ReadObject(ms);
        }
    }
}

[Serializable]
[DataContract]
public record class TicketRecord
{
    [DataMember]
    public string Key { get; set; }
    [DataMember]
    public string UserId { get; set; }
    [DataMember]
    public string Username { get; set; }
    [DataMember]
    public DateTime LastActivityUtc { get; set; }
    [DataMember]
    public byte[] Value { get; set; }
    [DataMember]
    public DateTimeOffset? ExpiresUtc { get; set; }
}
