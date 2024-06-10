using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Users;

/// <summary>
/// Interface <c>IUserToExternalLoginProvider</c> allow to create services used to decide
/// when link an existing local user account to the login informations that comes from an external
/// login system like OpenId or GitHub.
/// </summary>
public interface IUserToExternalLoginProvider
{
    /// <summary>
    /// Method <c>CanHandle</c> extabilish if the service that implement this interface can handle
    /// a kind of external login.
    /// </summary>
    /// <param name="info">
    /// external login information with type and other data to extabilish if
    /// service can hadle this kind of external login.
    /// </param>
    /// <returns>
    /// True, if the service can handle the external login by the informations in the parameter. False, instead.
    /// </returns>
    bool CanHandle(ExternalLoginInfo info);

    /// <summary>
    /// Method <c>GetUserAsync</c> return a local user account that match an identifier used by the
    /// service to return an existing local user account.
    /// </summary>
    /// <param name="info">
    /// external login information to extabilish if
    /// exist a local user account that match login data.
    /// </param>
    /// <returns>
    /// an object that implement <c>IUser</c>, if the exit a local account that match the external login data. Null, instead.
    /// </returns>
    Task<IUser> GetUserAsync(ExternalLoginInfo info);

    /// <summary>
    /// Method <c>GetIdentifierKey</c> return the value of the login data used by the service to
    /// match a local user account to the external login informations.
    /// </summary>
    /// <param name="info">
    /// external login information.
    /// </param>
    /// <returns>
    /// a <c>string</c> that is used like an identifier to match an existing local user account.
    /// Used in the related views to render the value that cause the page display.
    /// </returns>
    string GetIdentifierKey(ExternalLoginInfo info);
}
