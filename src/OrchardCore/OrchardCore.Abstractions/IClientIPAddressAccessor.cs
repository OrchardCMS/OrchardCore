using System.Net;
using System.Threading.Tasks;

namespace OrchardCore;

public interface IClientIPAddressAccessor
{
    Task<IPAddress> GetIPAddressAsync();
}
