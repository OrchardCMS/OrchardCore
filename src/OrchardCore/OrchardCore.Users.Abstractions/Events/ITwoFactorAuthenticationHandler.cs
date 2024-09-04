namespace OrchardCore.Users.Events;

public interface ITwoFactorAuthenticationHandler
{
    /// <summary>
    /// Checks if the two-factor authentication should be required for the given user or not.
    /// </summary>
    /// <param name="user">An instance of the user to evaluate.</param>
    /// <returns>true if the two-factor authentication is required otherwise false.</returns>
    Task<bool> IsRequiredAsync(IUser user);
}
