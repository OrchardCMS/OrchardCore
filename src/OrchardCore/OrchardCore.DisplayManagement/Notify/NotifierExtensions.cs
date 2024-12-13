using Microsoft.AspNetCore.Mvc.Localization;

namespace OrchardCore.DisplayManagement.Notify;

public static class NotifierExtensions
{
    /// <summary>
    /// Adds a new UI notification of type Information.
    /// </summary>
    /// <seealso cref="INotifier.AddAsync"/>
    /// <param name="notifier">The <see cref="INotifier"/>.</param>
    /// <param name="message">A localized message to display.</param>
    public static ValueTask InformationAsync(this INotifier notifier, LocalizedHtmlString message)
        => notifier.AddAsync(NotifyType.Information, message);

    /// <summary>
    /// Adds a new UI notification of type Warning.
    /// </summary>
    /// <seealso cref="INotifier.AddAsync"/>
    /// <param name="notifier">The <see cref="INotifier"/>.</param>
    /// <param name="message">A localized message to display.</param>
    public static ValueTask WarningAsync(this INotifier notifier, LocalizedHtmlString message)
        => notifier.AddAsync(NotifyType.Warning, message);

    /// <summary>
    /// Adds a new UI notification of type Error.
    /// </summary>
    /// <seealso cref="INotifier.AddAsync"/>
    /// <param name="notifier">The <see cref="INotifier"/>.</param>
    /// <param name="message">A localized message to display.</param>
    public static ValueTask ErrorAsync(this INotifier notifier, LocalizedHtmlString message)
        => notifier.AddAsync(NotifyType.Error, message);

    /// <summary>
    /// Adds a new UI notification of type Success.
    /// </summary>
    /// <seealso cref="INotifier.AddAsync"/>
    /// <param name="notifier">The <see cref="INotifier"/>.</param>
    /// <param name="message">A localized message to display.</param>
    public static ValueTask SuccessAsync(this INotifier notifier, LocalizedHtmlString message)
        => notifier.AddAsync(NotifyType.Success, message);
}
