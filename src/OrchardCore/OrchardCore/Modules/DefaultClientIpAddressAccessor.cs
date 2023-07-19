using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Modules;

public class DefaultClientIpAddressAccessor : IClientIpAddressAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DefaultClientIpAddressAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<IPAddress> GetIpAddressAsync()
    {
        var address = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress;

        if (address != null)
        {
            if (IPAddress.IsLoopback(address))
            {
                address = IPAddress.Loopback;
            }
        }

        return Task.FromResult(address);
    }
}
