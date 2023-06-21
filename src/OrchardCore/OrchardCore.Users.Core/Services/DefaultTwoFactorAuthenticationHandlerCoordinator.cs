using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.Services;

public class DefaultTwoFactorAuthenticationHandlerCoordinator : ITwoFactorAuthenticationHandlerCoordinator
{
    private readonly IEnumerable<ITwoFactorAuthenticationHandler> _handlers;

    public DefaultTwoFactorAuthenticationHandlerCoordinator(
        IEnumerable<ITwoFactorAuthenticationHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task<bool> IsRequiredAsync()
    {
        foreach (var handler in _handlers)
        {
            if (await handler.IsRequiredAsync())
            {
                return true;
            }
        }

        return false;
    }
}
