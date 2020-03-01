using System;
using System.Collections.Generic;
using System.Text;
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

            var initialEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path", "contained")
            };

            entries.AddEntries(initialEntries);

            // Act
            var result = entries.TryGetEntryByPath("/contained-path", out var containedEntry);

            // Test
            Assert.True(result);
            Assert.Equal("contained", containedEntry.ContainedContentItemId);
        }

        [Fact]
        public void ShouldGetEntryByContainedContentItemId()
        {
            // Setup
            var entries = new AutorouteEntries();

            var initialEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path", "contained")
            };

            entries.AddEntries(initialEntries);

            // Act
            var result = entries.TryGetEntryByContentItemId("contained", out var containedEntry);

            // Test
            Assert.True(result);
            Assert.Equal("/contained-path", containedEntry.Path);
        }

        [Fact]
        public void RemovesContainedEntriesWhenContainerRemoved()
        {
            // Setup
            var entries = new AutorouteEntries();

            var initialEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path", "contained")
            };

            entries.AddEntries(initialEntries);

            // Act
            entries.RemoveEntry("container", "container-path");
            var result = entries.TryGetEntryByPath("/contained-path", out var entry);

            // Test
            Assert.False(result);
        }

        [Fact]
        public void RemovesContainedEntriesWhenDeleted()
        {
            // Setup
            var entries = new AutorouteEntries();

            var initialEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path1", "contained1"),
                new AutorouteEntry("container", "contained-path2", "contained2")
            };

            entries.AddEntries(initialEntries);

            // Act
            var updatedEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path1", "contained1")
            };

            entries.AddEntries(updatedEntries);
            var result = entries.TryGetEntryByPath("/contained-path2", out var entry);

            // Test
            Assert.False(result);
        }

        [Fact]
        public void RemovesOldContainedPaths()
        {
            // Setup
            var entries = new AutorouteEntries();

            var initialEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path-old", "contained")
            };

            entries.AddEntries(initialEntries);

            // Act
            var updatedEntries = new List<AutorouteEntry>()
            {
                new AutorouteEntry("container", "container-path"),
                new AutorouteEntry("container", "contained-path-new", "contained")
            };

            entries.AddEntries(updatedEntries);
            var result = entries.TryGetEntryByPath("/contained-path-old", out var entry);

            // Test
            Assert.False(result);
        }

        [Fact]
        public void RemovesOldPaths()
        {
            // Setup
            var entries = new AutorouteEntries();

            entries.AddEntry("container", "container-path");

            // Act
            entries.RemoveEntry("container", "container-path");
            var result = entries.TryGetEntryByPath("/container-path", out var entry);

            // Test
            Assert.False(result);
        }
    }
}
