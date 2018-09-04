using System.Collections.Generic;
using System.Linq;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.WebHooks.Services.Events;
using Xunit;

namespace OrchardCore.Tests.WebHooks
{
    public class ContentWebHookEventsTests
    {
        [Fact]
        public void GenericContentEventsOverrideContentTypeEvents()
        {
            var typeDefinitions = new List<ContentTypeDefinition>
            {
                new ContentTypeDefinition("menu", "Menu", Enumerable.Empty<ContentTypePartDefinition>(), JObject.Parse("{ \"Creatable\": true }")),
                new ContentTypeDefinition("page", "Page", Enumerable.Empty<ContentTypePartDefinition>(), JObject.Parse("{ \"Creatable\": true }"))
            };

            var contentDefinitionManager = new Mock<IContentDefinitionManager>();
            contentDefinitionManager.Setup(x => x.ListTypeDefinitions()).Returns(typeDefinitions);

            var contentEvents = new ContentWebHookEvents(contentDefinitionManager.Object);

            var events = new List<string>
            {
                "content.created",
                "content.updated",
                "content.updated",
                "menu.updated",
                "page.created",
                "page.updated",
                "page.published"
            };

            var normalizedEvents = contentEvents.NormalizeEvents(events);

            var expectedEvents = new HashSet<string>
            {
                "content.created",
                "content.updated",
                "page.published"
            };

            Assert.Equal(expectedEvents, normalizedEvents);
        }
    }
}