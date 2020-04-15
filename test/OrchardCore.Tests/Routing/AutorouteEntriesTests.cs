using System.Collections.Generic;
using OrchardCore.Autoroute.Services;
using OrchardCore.ContentManagement.Routing;
using Xunit;

namespace OrchardCore.Tests.Routing
{
    public class AutorouteEntriesTests
    {
        [Fact]
        public void ShouldGetContainedEntryByPath()
        {
            // Setup
            var entries = new AutorouteEntries();
            var document = new AutorouteDocument();

            var initialEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path", "contained")
            };

            entries.AddEntries(document, initialEntries);

            // Act
            var result = entries.TryGetEntryByPath(document, "/contained-path", out var containedEntry);

            // Test
            Assert.True(result);
            Assert.Equal("contained", containedEntry.ContainedContentItemId);
        }

        [Fact]
        public void ShouldGetEntryByContainedContentItemId()
        {
            // Setup
            var document = new AutorouteDocument();
            var entries = new AutorouteEntries();

            var initialEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path", "contained")
            };

            entries.AddEntries(document, initialEntries);

            // Act
            var result = entries.TryGetEntryByContentItemId(document, "contained", out var containedEntry);

            // Test
            Assert.True(result);
            Assert.Equal("/contained-path", containedEntry.Path);
        }

        [Fact]
        public void RemovesContainedEntriesWhenContainerRemoved()
        {
            // Setup
            var entries = new AutorouteEntries();
            var document = new AutorouteDocument();

            var initialEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path", "contained")
            };

            entries.AddEntries(document, initialEntries);

            // Act
            entries.RemoveEntries(document, new[] { new AutorouteEntry("container", "container-path", null, null) });
            var result = entries.TryGetEntryByPath(document, "/contained-path", out _);

            // Test
            Assert.False(result);
        }

        [Fact]
        public void RemovesContainedEntriesWhenDeleted()
        {
            // Setup
            var entries = new AutorouteEntries();
            var document = new AutorouteDocument();

            var initialEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path1", "contained1"),
                new AutorouteEntry("container", "contained-path2", "contained2")
            };

            entries.AddEntries(document, initialEntries);

            // Act
            var updatedEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path1", "contained1")
            };

            entries.AddEntries(document, updatedEntries);
            var result = entries.TryGetEntryByPath(document, "/contained-path2", out _);

            // Test
            Assert.False(result);
        }

        [Fact]
        public void RemovesOldContainedPaths()
        {
            // Setup
            var entries = new AutorouteEntries();
            var document = new AutorouteDocument();

            var initialEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path-old", "contained")
            };

            entries.AddEntries(document, initialEntries);

            // Act
            var updatedEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path-new", "contained")
            };

            entries.AddEntries(document, updatedEntries);
            var result = entries.TryGetEntryByPath(document, "/contained-path-old", out _);

            // Test
            Assert.False(result);
        }

        [Fact]
        public void RemovesOldPaths()
        {
            // Setup
            var entries = new AutorouteEntries();
            var document = new AutorouteDocument();

            entries.AddEntries(document, new[] { new AutorouteEntry("container", "container-path", null, null) });

            // Act
            entries.RemoveEntries(document, new[] { new AutorouteEntry("container", "container-path", null, null) });
            var result = entries.TryGetEntryByPath(document, "/container-path", out _);

            // Test
            Assert.False(result);
        }
    }
}
