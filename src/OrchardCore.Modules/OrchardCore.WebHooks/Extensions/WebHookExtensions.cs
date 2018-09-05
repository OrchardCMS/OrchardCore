using OrchardCore.WebHooks.Abstractions.Models;
using OrchardCore.WebHooks.Services.Events;

namespace OrchardCore.WebHooks.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="WebHook"/>.
    /// </summary>
    public static class WebHookExtensions
    {
        /// <summary>
        /// Determines whether a given <paramref name="eventName"/> matches the subscribed events for a given <see cref="WebHook"/>.
        /// The action can either match an event directly or match a wildcard.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> instance to operate on.</param>
        /// <param name="eventName">The topic to match against the subscribed <paramref name="webHook"/> topics.</param>
        /// <returns><c>true</c> if the <paramref name="eventName"/> matches, otherwise <c>false</c>.</returns>
        public static bool MatchesEvent(this WebHook webHook, string eventName)
        {
            return webHook != null && (webHook.Events.Contains(WildcardWebHookEvent.WildcardEvent.Name) || webHook.Events.Contains(eventName));
        }
    }
}