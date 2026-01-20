using OrchardCore.Users.Events;

namespace OrchardCore.Users.Services;

public class DefaultTwoFactorAuthenticationHandlerCoordinator : ITwoFactorAuthenticationHandlerCoordinator
{
    private readonly IEnumerable<ITwoFactorAuthenticationHandler> _twoFactorAuthenticationHandlers;

    private bool? _isRequired;

    public DefaultTwoFactorAuthenticationHandlerCoordinator(
        IEnumerable<ITwoFactorAuthenticationHandler> twoFactorAuthenticationHandlers)
    {
        _twoFactorAuthenticationHandlers = twoFactorAuthenticationHandlers;
    }

    public async Task<bool> IsRequiredAsync(IUser user)
    {
        if (_isRequired is null)
        {
            foreach (var handler in _twoFactorAuthenticationHandlers)
            {
                if (await handler.IsRequiredAsync(user))
                {
                    _isRequired = true;

                    break;
                }
            }
        }

        return _isRequired ??= false;
    }
}
