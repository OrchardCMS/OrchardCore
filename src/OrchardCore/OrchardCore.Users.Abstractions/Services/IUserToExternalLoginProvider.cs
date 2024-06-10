using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Users;

/// <summary>
/// Provides a methods to link a local user with an externally authenticated user during registration.
/// </summary>
public interface IUserToExternalLoginProvider
{
    /// <summary>
    /// Checks of the implement can handle the given external login information.
    /// </summary>
    /// <param name="info">The external login information.</param>
    /// <returns>
    /// True if the service can handle the given external login information. False otherwise.
    /// </returns>
    bool CanHandle(ExternalLoginInfo info);

    /// <summary>
    /// <summary>
    /// Retrieves a local user account that corresponds to the provided external login information, if one exists.
    /// </summary>
    /// <param name="info">The external login information.</param>
    /// <returns>
    /// An instance of <see cref="IUser" /> if there's a local account matching the external login data; otherwise, null.
    /// </returns>
    Task<IUser> GetUserAsync(ExternalLoginInfo info);

    /// <summary>
    /// Method <c>GetIdentifierKey</c> return the value of the login data used by the service to
    /// match a local user account to the external login informations.
    /// </summary>
    /// <param name="info">
    /// external login informations.
    /// </param>
    /// <returns>
    /// a <c>string</c> that is used like an identifier to match an existing local user account.
    /// Used in the related views to render the value that cause the page display.
    /// </returns>
    string GetIdentifierKey(ExternalLoginInfo info);
}
