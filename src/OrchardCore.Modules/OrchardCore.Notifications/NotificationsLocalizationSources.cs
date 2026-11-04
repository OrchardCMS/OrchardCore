using OrchardCore.Localization;

namespace OrchardCore.Notifications;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Notifications.NotificationsLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class NotificationsLocalizationSources
{
    /// <summary>Gets the "Notifications" activity category.</summary>
    public static readonly LocalizationSource Notifications = LocalizationSource.Create("Notifications", typeof(NotificationsLocalizationSources));
}
