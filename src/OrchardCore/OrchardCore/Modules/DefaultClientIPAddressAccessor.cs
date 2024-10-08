using System.Net;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Modules;

public class DefaultClientIPAddressAccessor : IClientIPAddressAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DefaultClientIPAddressAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<string> GetIPAddressAsync()
    {
        string ip = string.Empty;
        // Check for X-Forwarded-For header
        var forwardedFor = _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // The client IP will be the first IP in the list
            ip = forwardedFor.Split(',')[0].Trim();
        }
        else
        {
            // If X-Forwarded-For is not available, use the remote IP address
            var address = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress;

            if (address != null)
            {
                if (IPAddress.IsLoopback(address))
                {
                    address = IPAddress.Loopback;
                }

                ip = address.ToString();
            }
        }
        
        return Task.FromResult(ip);
    }
}
