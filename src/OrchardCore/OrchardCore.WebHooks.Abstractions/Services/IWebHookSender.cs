using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.WebHooks.Abstractions.Models;

namespace OrchardCore.WebHooks.Abstractions.Services
{
    /// <summary>
    /// Provides an abstraction for sending out WebHooks as provided by <see cref="IWebHookManager"/>. Implementation
    /// can control the format of the WebHooks as well as how they are sent including retry policies and error handling.
    /// </summary>
    public interface IWebHookSender
    {
        /// <summary>
        /// Sends out the given collection of <paramref name="webHooks"/> using whatever mechanism defined by the
        /// <see cref="IWebHookSender"/> implementation.
        /// </summary>
        /// <param name="webHooks">The collection of <see cref="WebHook"/> instances to process.</param>
        /// <param name="context">The context for the webhook notification.</param>
        Task SendNotificationsAsync(IEnumerable<WebHook> webHooks, WebHookNotificationContext context);
    }
}