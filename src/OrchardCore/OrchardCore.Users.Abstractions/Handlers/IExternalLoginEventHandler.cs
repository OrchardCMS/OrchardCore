namespace OrchardCore.Users.Handlers;

/// <summary>
/// Contract for external login events, this occurs when a tenant is set up.
/// </summary>
public interface IExternalLoginEventHandler
{
    /// <summary>
    /// Occurs when the username is generated.
    /// </summary>
    /// <param name="provider">The login provider.</param>
    /// <param name="claims">A <see cref="SerializableClaim"/>s.</param>
    /// <return>A username.</return>
    Task<string> GenerateUserName(string provider, IEnumerable<SerializableClaim> claims);

    /// <summary>
    /// Occurs when the user is updated.
    /// </summary>
    /// <param name="context">The <see cref="UpdateUserContext"/>.</param>
    Task UpdateUserAsync(UpdateUserContext context);
}
