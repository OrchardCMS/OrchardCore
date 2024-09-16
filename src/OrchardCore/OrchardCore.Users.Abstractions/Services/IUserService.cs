using System.Security.Claims;

namespace OrchardCore.Users.Services;

/// <summary>
/// Contract that provides an abstraction for common user operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Authenticates the user credentials.
    /// </summary>
    /// <param name="usernameOrEmail">The username or email address.</param>
    /// <param name="password">The user password.</param>
    /// <param name="reportError">The error reported in case failure happened during the authentication process.</param>
    /// <returns>A <see cref="IUser"/> that represents an authenticated user.</returns>
    Task<IUser> AuthenticateAsync(string usernameOrEmail, string password, Action<string, string> reportError);

    /// <summary>
    /// Creates a user.
    /// </summary>
    /// <param name="user">The <see cref="IUser"/>.</param>
    /// <param name="password">The user password.</param>
    /// <param name="reportError">The error reported in case failure happened during the creation process.</param>
    /// <returns>A <see cref="IUser"/> represents a created user.</returns>
    Task<IUser> CreateUserAsync(IUser user, string password, Action<string, string> reportError);

    /// <summary>
    /// Change a user email.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="newEmail">The new email.</param>
    /// <param name="reportError">The error reported in case failure happened during the creation process.</param>
    /// <returns>Returns <c>true</c> if the email has been changed, otherwise <c>false</c>.</returns>
    Task<bool> ChangeEmailAsync(IUser user, string newEmail, Action<string, string> reportError);

    /// <summary>
    /// Change a user password.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="currentPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <param name="reportError"></param>
    /// <returns>Returns <c>true</c> if the password has been changed, otherwise <c>false</c>.</returns>
    Task<bool> ChangePasswordAsync(IUser user, string currentPassword, string newPassword, Action<string, string> reportError);

    /// <summary>
    /// Gets the authenticated user from a given <see cref="ClaimsPrincipal"/>.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/>.</param>
    /// <returns>A <see cref="IUser"/> represents the authenticated user.</returns>
    Task<IUser> GetAuthenticatedUserAsync(ClaimsPrincipal principal);

    /// <summary>
    /// Gets the user with a specified username or email address.
    /// </summary>
    /// <param name="usernameOrEmail">The username or email address.</param>
    /// <returns>The <see cref="IUser"/> represents the retrieved user.</returns>
    Task<IUser> GetUserAsync(string usernameOrEmail);

    /// <summary>
    /// Gets the user with a specified ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A <see cref="IUser"/> represents a retrieved user.</returns>
    Task<IUser> GetUserByUniqueIdAsync(string userId);

    /// <summary>
    /// Get a forgotten password for a specified user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A <see cref="IUser"/> represents a user with forgotten password.</returns>
    Task<IUser> GetForgotPasswordUserAsync(string userId);

    /// <summary>
    /// Resets the user password.
    /// </summary>
    /// <param name="usernameOrEmail">The username or email address.</param>
    /// <param name="resetToken">The token used to reset the password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <param name="reportError">The error reported in case failure happened during the reset process.</param>
    /// <returns>Returns <c>true</c> if the password reset, otherwise <c>false</c>.</returns>
    Task<bool> ResetPasswordAsync(string usernameOrEmail, string resetToken, string newPassword, Action<string, string> reportError);

    /// <summary>
    /// Creates a <see cref="ClaimsPrincipal"/> for a given user.
    /// </summary>
    /// <param name="user">The <see cref="IUser"/>.</param>
    /// <returns>The <see cref="ClaimsPrincipal"/>.</returns>
    Task<ClaimsPrincipal> CreatePrincipalAsync(IUser user);
}
