using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Localization;

namespace OrchardCore.DisplayManagement.Notify
{
    /// <summary>
    /// Notification manager for UI notifications
    /// </summary>
    /// <remarks>
    /// Where such notifications are displayed depends on the theme used. Default themes contain a
    /// Messages zone for this.
    /// </remarks>
    public interface INotifier
    {
        /// <summary>
        /// Adds a new UI notification
        /// </summary>
        /// <param name="type">
        /// The type of the notification (notifications with different types can be displayed differently)</param>
        /// <param name="message">A localized message to display</param>
        [Obsolete("This method will be removed in a later version. Use AddAsync()")]
        void Add(NotifyType type, LocalizedHtmlString message);

        /// <summary>
        /// Adds a new UI notification asynchronously.
        /// </summary>
        /// <param name="type">
        /// The type of the notification (notifications with different types can be displayed differently)</param>
        /// <param name="message">A localized message to display</param>
        /// <remarks>
        /// Added with a default interface implementation for backwards compatability.
        /// </remarks>
        ValueTask AddAsync(NotifyType type, LocalizedHtmlString message)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Add(type, message);
#pragma warning restore CS0618 // Type or member is obsolete

            return new ValueTask();
        }

        /// <summary>
        /// Get all notifications added
        /// </summary>
        IList<NotifyEntry> List();
    }
}
