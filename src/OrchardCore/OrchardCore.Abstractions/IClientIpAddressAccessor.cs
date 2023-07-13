using System.Net;
using System.Threading.Tasks;

namespace OrchardCore;

public interface IClientIpAddressAccessor
{
    Task<IPAddress> GetIpAddressAsync();
}
