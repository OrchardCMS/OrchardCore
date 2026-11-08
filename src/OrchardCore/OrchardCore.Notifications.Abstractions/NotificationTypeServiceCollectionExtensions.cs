using OrchardCore.Localization;
using OrchardCore.Notifications;

namespace Microsoft.Extensions.DependencyInjection;

public static class NotificationTypeServiceCollectionExtensions
{
    /// <summary>
    /// Registers a type of notification, to give it a display name, list it in the type filter of the
    /// notification center and let users choose their notification methods for it.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The technical name of the type, set on <see cref="INotificationMessage.Type"/>.</param>
    /// <param name="displayName">The name shown to users.</param>
    /// <param name="configure">An optional delegate to configure the type, for example its default methods.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNotificationType(
        this IServiceCollection services,
        string name,
        LocalizationSource displayName,
        Action<NotificationTypeDescriptor> configure = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(displayName);

        services.Configure<NotificationTypeOptions>(options => options.Add(name, displayName, configure));

        return services;
    }
}
