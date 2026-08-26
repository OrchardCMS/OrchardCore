using System;
using OrchardCore.Autoroute.Handlers;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Tests.Modules.OrchardCore.Autoroute.Handlers;

public class AutoroutePartHandlerTests
{
    [Fact]
    public async Task GenerateRelativeUniquePathAsync_ShouldReturnUniquePath_WhenPathAlreadyExists()
    {
        // Arrange
        var expectedPath = "existing-path";
        var contextContentItemId = "context-item-id";
        var existingEntries = new List<AutorouteEntry>
        {
            new AutorouteEntry("id", expectedPath, "other-item-id"),
            new AutorouteEntry("id", $"{expectedPath}-1", "other-item-id"),
        };

        var context = new AutoroutePart
        {
            ContentItem = new global::OrchardCore.ContentManagement.ContentItem
            {
                ContentItemId = contextContentItemId,
            },
        };

        // Act
        var actualPath = AutoroutePartHandler.GenerateRelativeUniquePath(existingEntries, expectedPath, context);

        // Assert
        Assert.Equal($"{expectedPath}-2", actualPath);
    }
    [Fact]
    public async Task GenerateRelativeUniquePathAsync_ConcurrentCalls_ReturnsUniquePaths()
    {
        // Arrange
        var basePath = "test-path";
        var contextContentItemId = "context-item-id";
        var existingEntries = new List<AutorouteEntry>
        {
            new AutorouteEntry("id", basePath, "other-item-id"),
            new AutorouteEntry("id", $"{basePath}-1", "other-item-id"),
        };

        var context = new AutoroutePart
        {
            ContentItem = new global::OrchardCore.ContentManagement.ContentItem
            {
                ContentItemId = contextContentItemId,
            },
        };

        // Act: call the method multiple times concurrently sharing the SAME entries list
        var sharedEntries = new List<AutorouteEntry>(existingEntries);
        var tasks = Enumerable.Range(0, 5).Select(i => Task.Run(() =>
            AutoroutePartHandler.GenerateRelativeUniquePath(sharedEntries, basePath, context))
        ).ToArray();

        await Task.WhenAll(tasks);

        var results = tasks.Select(t => t.Result).ToArray();

        // Assert
        Assert.Equal(5, results.Length);
        Assert.All(results, r => Assert.StartsWith($"{basePath}-", r));
        Assert.Distinct(results);
    }
    [Fact]
    public void Test1() {
        Assert.True(true);
    }
}