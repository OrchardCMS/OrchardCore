using System.Net;

namespace OrchardCore;

public interface IClientIPAddressAccessor
{
    Task<IPAddress> GetIPAddressAsync();
}
