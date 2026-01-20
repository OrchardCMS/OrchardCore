namespace OrchardCore.Notifications;

/// <summary>
/// Represents a contract for accessing the <see cref="INotificationMethodProvider"/>.
/// </summary>
public interface INotificationMethodProviderAccessor
{
    /// <summary>
    /// Gets the registered notification method providers.
    /// </summary>
    /// <param name="notify">The notifiable object.</param>
    /// <returns>A list of <see cref="INotificationMethodProvider"/>.</returns>
    Task<IEnumerable<INotificationMethodProvider>> GetProvidersAsync(object notify);
}
