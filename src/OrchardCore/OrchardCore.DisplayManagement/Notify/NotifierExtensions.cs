using Microsoft.AspNetCore.Mvc.Localization;

namespace OrchardCore.DisplayManagement.Notify;

public static class NotifierExtensions
{
    /// <summary>
    /// Adds a new UI notification of type Information.
    /// </summary>
    /// <seealso cref="INotifier.AddAsync(NotifyType, LocalizedHtmlString, NotifyContext)"/>
    /// <param name="notifier">The <see cref="INotifier"/>.</param>
    /// <param name="message">A localized message to display.</param>
    public static ValueTask InformationAsync(this INotifier notifier, LocalizedHtmlString message)
        => notifier.AddAsync(NotifyType.Information, message);

    /// <summary>
    /// Adds a new UI notification of type Information with an auto-dismiss delay.
    /// </summary>
    /// <seealso cref="INotifier.AddAsync(NotifyType, LocalizedHtmlString, NotifyContext)"/>
    /// <param name="notifier">The <see cref="INotifier"/>.</param>
    /// <param name="message">A localized message to display.</param>
    /// <param name="milliseconds">The auto-dismiss delay in milliseconds.</param>
    public static ValueTask InformationAsync(this INotifier notifier, LocalizedHtmlString message, int milliseconds)
        => notifier.AddAsync(NotifyType.Information, message, new NotifyContext { Milliseconds = milliseconds });

    /// <summary>
    /// Adds a new UI notification of type Warning.
    /// </summary>
    /// <seealso cref="INotifier.AddAsync(NotifyType, LocalizedHtmlString, NotifyContext)"/>
    /// <param name="notifier">The <see cref="INotifier"/>.</param>
    /// <param name="message">A localized message to display.</param>
    public static ValueTask WarningAsync(this INotifier notifier, LocalizedHtmlString message)
        => notifier.AddAsync(NotifyType.Warning, message);

    /// <summary>
    /// Adds a new UI notification of type Warning with an auto-dismiss delay.
    /// </summary>
    /// <seealso cref="INotifier.AddAsync(NotifyType, LocalizedHtmlString, NotifyContext)"/>
    /// <param name="notifier">The <see cref="INotifier"/>.</param>
    /// <param name="message">A localized message to display.</param>
    /// <param name="milliseconds">The auto-dismiss delay in milliseconds.</param>
    public static ValueTask WarningAsync(this INotifier notifier, LocalizedHtmlString message, int milliseconds)
        => notifier.AddAsync(NotifyType.Warning, message, new NotifyContext { Milliseconds = milliseconds });

    /// <summary>
    /// Adds a new UI notification of type Error.
    /// </summary>
    /// <seealso cref="INotifier.AddAsync(NotifyType, LocalizedHtmlString, NotifyContext)"/>
    /// <param name="notifier">The <see cref="INotifier"/>.</param>
    /// <param name="message">A localized message to display.</param>
    public static ValueTask ErrorAsync(this INotifier notifier, LocalizedHtmlString message)
        => notifier.AddAsync(NotifyType.Error, message);

    /// <summary>
    /// Adds a new UI notification of type Error with an auto-dismiss delay.
    /// </summary>
    /// <seealso cref="INotifier.AddAsync(NotifyType, LocalizedHtmlString, NotifyContext)"/>
    /// <param name="notifier">The <see cref="INotifier"/>.</param>
    /// <param name="message">A localized message to display.</param>
    /// <param name="milliseconds">The auto-dismiss delay in milliseconds.</param>
    public static ValueTask ErrorAsync(this INotifier notifier, LocalizedHtmlString message, int milliseconds)
        => notifier.AddAsync(NotifyType.Error, message, new NotifyContext { Milliseconds = milliseconds });

    /// <summary>
    /// Adds a new UI notification of type Success.
    /// </summary>
    /// <seealso cref="INotifier.AddAsync(NotifyType, LocalizedHtmlString, NotifyContext)"/>
    /// <param name="notifier">The <see cref="INotifier"/>.</param>
    /// <param name="message">A localized message to display.</param>
    public static ValueTask SuccessAsync(this INotifier notifier, LocalizedHtmlString message)
        => notifier.AddAsync(NotifyType.Success, message, new NotifyContext { Milliseconds = 5000 });

    /// <summary>
    /// Adds a new UI notification of type Success with an auto-dismiss delay.
    /// </summary>
    /// <seealso cref="INotifier.AddAsync(NotifyType, LocalizedHtmlString, NotifyContext)"/>
    /// <param name="notifier">The <see cref="INotifier"/>.</param>
    /// <param name="message">A localized message to display.</param>
    /// <param name="milliseconds">The auto-dismiss delay in milliseconds.</param>
    public static ValueTask SuccessAsync(this INotifier notifier, LocalizedHtmlString message, int milliseconds)
        => notifier.AddAsync(NotifyType.Success, message, new NotifyContext { Milliseconds = milliseconds });
}
