using System.Threading.Tasks;

namespace OrchardCore.Users.Events;

public interface ITwoFactorAuthenticationHandler
{
    /// <summary>
    /// Checks if the two-factor authentication should be required or not.
    /// </summary>
    /// <returns>true if the two-factor authentication is required otherwise false.</returns>
    Task<bool> IsRequiredAsync();
}
