using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.WebHooks.Abstractions.Models;
using OrchardCore.WebHooks.Models;
using OrchardCore.WebHooks.Abstractions.Services;

namespace OrchardCore.WebHooks.Services.Events
{
    public class ContentWebHookEvents : IWebHookEventProvider
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentWebHookEvents(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public IEnumerable<WebHookEvent> GetEvents()
        {
            // Only allow the user to manage events for content types that are creatabale in the UI
            var typeDefinitions = _contentDefinitionManager.ListTypeDefinitions()
                .Where(definition => definition.Settings.ToObject<ContentTypeSettings>().Creatable);
            
            // Add events for each content type e.g. article.created
            foreach (var typeDefinition in typeDefinitions)
            {
                foreach (var contentEvent in ContentEvents.AllEvents)
                {
                    yield return CreateEvent(typeDefinition.Name.ToLower(), contentEvent, typeDefinition.DisplayName, category: "ContentType");
                }
            }

            // Add events for all content type events e.g. content.created, content.published
            foreach (var contentEvent in ContentEvents.AllEvents)
            {
                yield return CreateEvent("content", contentEvent, category: "Content");
            }
        }

        public IEnumerable<string> NormalizeEvents(IEnumerable<string> submittedEvents)
        {
            if(submittedEvents == null) throw new ArgumentNullException(nameof(submittedEvents));
            
            var normalizedEvents = new HashSet<string>();
            var events = GetEvents().ToList();
            
            // Verify the events only consist of those defined by this provider 
            foreach (var @event in submittedEvents)
            {
                if (events.Any(e => e.Name == @event))
                {
                    normalizedEvents.Add(@event);
                }
            }

            // When a content.{eventName} event is being subscribed to, remove all other *.{eventName}
            // since the 'content' event implies a subscription to all content events of that type.
            foreach (var contentEvent in ContentEvents.AllEvents)
            {
                if(normalizedEvents.Contains($"content.{contentEvent}"))
                {
                    normalizedEvents.RemoveWhere(e => !e.StartsWith("content.") && e.EndsWith($".{contentEvent}"));
                }
            }

            return normalizedEvents;
        }

        private WebHookEvent CreateEvent(string eventName, string subEventName, string eventDisplayName = null, string category = null)
        {
            return new WebHookEvent($"{eventName}.{subEventName}", eventDisplayName, category: category);
        }
    }
}