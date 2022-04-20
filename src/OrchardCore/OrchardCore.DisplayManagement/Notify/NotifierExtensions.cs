using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Localization;

namespace OrchardCore.DisplayManagement.Notify
{
    public static class NotifierExtensions
    {
        /// <summary>
        /// Adds a new UI notification of type Information
        /// </summary>
        /// <seealso cref="INotifier.Add"/>
        /// <param name="notifier">The <see cref="INotifier"/></param>
        /// <param name="message">A localized message to display</param>
        [Obsolete("This method will be removed in a later version. Use InformationAsync()")]
        public static void Information(this INotifier notifier, LocalizedHtmlString message)
        {
            notifier.Add(NotifyType.Information, message);
        }

        /// <summary>
        /// Adds a new UI notification of type Information
        /// </summary>
        /// <seealso cref="INotifier.AddAsync"/>
        /// <param name="notifier">The <see cref="INotifier"/></param>
        /// <param name="message">A localized message to display</param>
        public static ValueTask InformationAsync(this INotifier notifier, LocalizedHtmlString message)
            => notifier.AddAsync(NotifyType.Information, message);

        /// <summary>
        /// Adds a new UI notification of type Warning
        /// </summary>
        /// <seealso cref="INotifier.Add"/>
        /// <param name="notifier">The <see cref="INotifier"/></param>
        /// <param name="message">A localized message to display</param>
        [Obsolete("This method will be removed in a later version. Use WarningAsync()")]
        public static void Warning(this INotifier notifier, LocalizedHtmlString message)
        {
            notifier.Add(NotifyType.Warning, message);
        }

        /// <summary>
        /// Adds a new UI notification of type Warning
        /// </summary>
        /// <seealso cref="INotifier.AddAsync"/>
        /// <param name="notifier">The <see cref="INotifier"/></param>
        /// <param name="message">A localized message to display</param>
        public static ValueTask WarningAsync(this INotifier notifier, LocalizedHtmlString message)
            => notifier.AddAsync(NotifyType.Warning, message);

        /// <summary>
        /// Adds a new UI notification of type Error
        /// </summary>
        /// <seealso cref="INotifier.Add"/>
        /// <param name="notifier">The <see cref="INotifier"/></param>
        /// <param name="message">A localized message to display</param>
        [Obsolete("This method will be removed in a later version. Use ErrorAsync()")]
        public static void Error(this INotifier notifier, LocalizedHtmlString message)
        {
            notifier.Add(NotifyType.Error, message);
        }

        /// <summary>
        /// Adds a new UI notification of type Error
        /// </summary>
        /// <seealso cref="INotifier.AddAsync"/>
        /// <param name="notifier">The <see cref="INotifier"/></param>
        /// <param name="message">A localized message to display</param>
        public static ValueTask ErrorAsync(this INotifier notifier, LocalizedHtmlString message)
            => notifier.AddAsync(NotifyType.Error, message);

        /// <summary>
        /// Adds a new UI notification of type Success
        /// </summary>
        /// <seealso cref="INotifier.Add"/>
        /// <param name="notifier">The <see cref="INotifier"/></param>
        /// <param name="message">A localized message to display</param>
        [Obsolete("This method will be removed in a later version. Use SuccessAsync()")]
        public static void Success(this INotifier notifier, LocalizedHtmlString message)
        {
            notifier.Add(NotifyType.Success, message);
        }

        /// <summary>
        /// Adds a new UI notification of type Success
        /// </summary>
        /// <seealso cref="INotifier.AddAsync"/>
        /// <param name="notifier">The <see cref="INotifier"/></param>
        /// <param name="message">A localized message to display</param>
        public static ValueTask SuccessAsync(this INotifier notifier, LocalizedHtmlString message)
            => notifier.AddAsync(NotifyType.Success, message);
    }
}
