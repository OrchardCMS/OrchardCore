namespace OrchardCore.Users.Events;

/// <summary>
/// Contract for logout events.
/// </summary>
public interface ILogoutFormEvent
{
    /// <summary>
    /// Occurs when the user is logging out, before the user is signed out.
    /// </summary>
    /// <param name="user">The <see cref="IUser"/> that is being signed out.</param>
    Task LoggingOutAsync(IUser user);

    /// <summary>
    /// Occurs when the user has logged out, after the user is signed out.
    /// </summary>
    /// <param name="user">The <see cref="IUser"/> that was signed out.</param>
    Task LoggedOutAsync(IUser user);
}
