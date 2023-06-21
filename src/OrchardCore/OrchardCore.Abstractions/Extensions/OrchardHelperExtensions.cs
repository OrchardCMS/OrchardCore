using System.Net;

namespace OrchardCore;

public static class OrchardHelperExtensions
{
    public static IPAddress GetClientIpAddress(this IOrchardHelper orchardHelper)
    {
        var address = orchardHelper.HttpContext?.Connection?.RemoteIpAddress;

        if (address != null)
        {
            if (IPAddress.IsLoopback(address))
            {
                address = IPAddress.Loopback;
            }
        }

        return address;
    }
}
