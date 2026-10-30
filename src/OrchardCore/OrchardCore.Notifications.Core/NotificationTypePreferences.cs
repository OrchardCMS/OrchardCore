using OrchardCore.Notifications.Models;

namespace OrchardCore.Notifications;

/// <summary>
/// Resolves the notification methods a user receives for a type of notification.
/// </summary>
public static class NotificationTypePreferences
{
    /// <summary>
    /// Gets whether a method is on for a type of notification: the choice of the user for the type when
    /// they made one, otherwise the default of the type. A method the user turned off for every type stays off.
    /// </summary>
    /// <param name="part">The preferences of the user, or <see langword="null"/>.</param>
    /// <param name="type">The type of notification.</param>
    /// <param name="method">The notification method.</param>
    /// <returns><see langword="true"/> when the user receives the type with the method.</returns>
    public static bool IsMethodEnabled(UserNotificationPreferencesPart part, NotificationTypeDescriptor type, string method)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (part?.Optout?.Contains(method, StringComparer.OrdinalIgnoreCase) == true)
        {
            return false;
        }

        return IsMethodEnabledForType(part, type, method);
    }

    /// <summary>
    /// Gets whether a method is on for a type of notification, without the methods the user turned off for every type.
    /// </summary>
    /// <param name="part">The preferences of the user, or <see langword="null"/>.</param>
    /// <param name="type">The type of notification.</param>
    /// <param name="method">The notification method.</param>
    /// <returns><see langword="true"/> when the method is on for the type.</returns>
    public static bool IsMethodEnabledForType(UserNotificationPreferencesPart part, NotificationTypeDescriptor type, string method)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (part?.Types is not null && part.Types.TryGetValue(type.Name, out var preference) && preference is not null)
        {
            if (preference.Optin?.Contains(method, StringComparer.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            if (preference.Optout?.Contains(method, StringComparer.OrdinalIgnoreCase) == true)
            {
                return false;
            }
        }

        return !type.DisabledMethods.Contains(method);
    }

    /// <summary>
    /// Records the methods a user turned on for a type of notification, keeping only the choices that differ
    /// from the defaults of the type, so a change of the defaults applies to the users who did not choose.
    /// </summary>
    /// <param name="part">The preferences of the user.</param>
    /// <param name="type">The type of notification.</param>
    /// <param name="methods">Every available method.</param>
    /// <param name="enabledMethods">The methods the user turned on for the type.</param>
    public static void SetEnabledMethods(UserNotificationPreferencesPart part, NotificationTypeDescriptor type, IEnumerable<string> methods, IEnumerable<string> enabledMethods)
    {
        ArgumentNullException.ThrowIfNull(part);
        ArgumentNullException.ThrowIfNull(type);

        var enabled = new HashSet<string>(enabledMethods ?? [], StringComparer.OrdinalIgnoreCase);
        var optin = new List<string>();
        var optout = new List<string>();

        foreach (var method in methods)
        {
            var isDefault = !type.DisabledMethods.Contains(method);
            var isEnabled = enabled.Contains(method);

            if (isEnabled && !isDefault)
            {
                optin.Add(method);
            }
            else if (!isEnabled && isDefault)
            {
                optout.Add(method);
            }
        }

        part.Types ??= new Dictionary<string, NotificationTypePreference>(StringComparer.OrdinalIgnoreCase);

        if (optin.Count == 0 && optout.Count == 0)
        {
            part.Types.Remove(type.Name);

            return;
        }

        part.Types[type.Name] = new NotificationTypePreference
        {
            Optin = optin.ToArray(),
            Optout = optout.ToArray(),
        };
    }
}
