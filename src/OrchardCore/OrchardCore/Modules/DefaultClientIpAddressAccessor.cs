using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Modules;

public class DefaultClientIPAddressAccessor : IClientIPAddressAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DefaultClientIPAddressAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<IPAddress> GetIPAddressAsync()
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
