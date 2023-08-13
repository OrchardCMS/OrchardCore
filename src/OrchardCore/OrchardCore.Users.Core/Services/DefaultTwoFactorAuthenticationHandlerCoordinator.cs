using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.Services;

public class DefaultTwoFactorAuthenticationHandlerCoordinator : ITwoFactorAuthenticationHandlerCoordinator
{
    private readonly IEnumerable<ITwoFactorAuthenticationHandler> _twoFactorAuthenticationHandlers;

    private bool? _isRequired = null;

    public DefaultTwoFactorAuthenticationHandlerCoordinator(
        IEnumerable<ITwoFactorAuthenticationHandler> twoFactorAuthenticationHandlers)
    {
        _twoFactorAuthenticationHandlers = twoFactorAuthenticationHandlers;
    }

    public async Task<bool> IsRequiredAsync()
    {
        if (_isRequired is null)
        {
            foreach (var handler in _twoFactorAuthenticationHandlers)
            {
                if (await handler.IsRequiredAsync())
                {
                    _isRequired = true;

                    break;
                }
            }
        }

        return _isRequired ??= false;
    }
}
