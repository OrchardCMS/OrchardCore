namespace OrchardCore.Users.Handlers;

/// <summary>
/// Contract for external login user operations.
/// </summary>
public interface IExternalLoginUserHandler
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
