using System.Threading.Tasks;

namespace OrchardCore.Users.Events;

public interface ITwoFactorAuthenticationHandler
{
    /// <summary>
    /// Checks if the Two Factor Authentication should be required or not.
    /// </summary>
    /// <returns>true is the Two Factor Authentication is required otherwise false.</returns>
    Task<bool> ShouldRequireAsync();
}
