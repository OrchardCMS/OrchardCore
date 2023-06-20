using System.Threading.Tasks;

namespace OrchardCore.Users.Events;

public interface ITwoFactorAuthenticationHandlerCoordinator
{
    /// <summary>
    /// Checks if the Two Factor Authentication should be required or not.
    /// </summary>
    /// <returns>true is any of the Two Factor Authentication providers require 2FA otherwise false.</returns>
    Task<bool> ShouldRequireAsync();
}
