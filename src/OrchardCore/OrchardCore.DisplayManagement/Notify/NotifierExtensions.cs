using Microsoft.AspNetCore.Mvc.Localization;

namespace OrchardCore.DisplayManagement.Notify
{
    public static class NotifierExtensions
    {
        /// <summary>
        /// Adds a new UI notification of type Information
        /// </summary>
        /// <seealso cref="INotifier.Add()"/>
        /// <param name="message">A localized message to display</param>
        public static void Information(this INotifier notifier, LocalizedHtmlString message)
        {
            notifier.Add(NotifyType.Information, message);
        }

        /// <summary>
        /// Adds a new UI notification of type Warning
        /// </summary>
        /// <seealso cref="INotifier.Add()"/>
        /// <param name="message">A localized message to display</param>
        public static void Warning(this INotifier notifier, LocalizedHtmlString message)
        {
            notifier.Add(NotifyType.Warning, message);
        }

        /// <summary>
        /// Adds a new UI notification of type Error
        /// </summary>
        /// <seealso cref="INotifier.Add()"/>
        /// <param name="message">A localized message to display</param>
        public static void Error(this INotifier notifier, LocalizedHtmlString message)
        {
            notifier.Add(NotifyType.Error, message);
        }

        /// <summary>
        /// Adds a new UI notification of type Success
        /// </summary>
        /// <seealso cref="INotifier.Add()"/>
        /// <param name="message">A localized message to display</param>
        public static void Success(this INotifier notifier, LocalizedHtmlString message)
        {
            notifier.Add(NotifyType.Success, message);
        }
    }
}
