using System.Threading.Tasks;

namespace OrchardCore.Users.Events;

public interface ITwoFactorAuthenticationHandlerCoordinator
{
    /// <summary>
    /// Checks if the two-factor authentication should be required or not.
    /// </summary>
    /// <returns>true if any of the two-factor authentication providers require 2FA otherwise false.</returns>
    Task<bool> IsRequiredAsync();
}
