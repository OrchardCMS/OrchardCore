namespace OrchardCore.Users.Events;

public interface ITwoFactorAuthenticationHandlerCoordinator
{
    /// <summary>
    /// Checks if the two-factor authentication should be required or not.
    /// </summary>
    /// <param name="user">An instance of the user to evaluate.</param>
    /// <returns>true if any of the two-factor authentication providers require 2FA otherwise false.</returns>
    Task<bool> IsRequiredAsync(IUser user);
}
