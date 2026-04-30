namespace OrchardCore.Security;

/// <summary>
/// Defines an event handler for when a role is updated. This allows other services to react
/// to role updates, such as invalidating user sessions when their roles change.
/// </summary>
public interface IRoleUpdatedEventHandler
{
    /// <summary>
    /// Handles logic to be executed when a role is updated.
    /// </summary>
    /// <param name="roleName">The name of the role that has been updated. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RoleUpdatedAsync(string roleName);
}
