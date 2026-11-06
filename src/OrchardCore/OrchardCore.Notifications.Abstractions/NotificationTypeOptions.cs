using OrchardCore.Localization;

namespace OrchardCore.Notifications;

/// <summary>
/// The types of notifications registered by the enabled features, see <see cref="NotificationTypeDescriptor"/>.
/// </summary>
public sealed class NotificationTypeOptions
{
    private readonly Dictionary<string, NotificationTypeDescriptor> _types = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the registered types, by name.
    /// </summary>
    public IReadOnlyDictionary<string, NotificationTypeDescriptor> Types => _types;

    /// <summary>
    /// Registers a type of notification, or replaces the type registered with the same name.
    /// </summary>
    /// <param name="name">The technical name of the type, stored on the notifications.</param>
    /// <param name="displayName">The name shown to users.</param>
    /// <param name="configure">An optional delegate to configure the type.</param>
    /// <returns>The registered type.</returns>
    public NotificationTypeDescriptor Add(string name, LocalizationSource displayName, Action<NotificationTypeDescriptor> configure = null)
    {
        var descriptor = new NotificationTypeDescriptor(name, displayName);

        configure?.Invoke(descriptor);

        _types[name] = descriptor;

        return descriptor;
    }
}
