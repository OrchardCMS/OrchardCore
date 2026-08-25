using System;
using OrchardCore.Autoroute.Handlers;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Autoroute.Tests.Handlers;

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
            ContentItem = new ContentManagement.ContentItem
            {
                ContentItemId = contextContentItemId,
            },
        };

        // Act
        var actualPath = AutoroutePartHandler.GenerateRelativeUniquePath(existingEntries, expectedPath, context);

        // Assert
        Assert.Equal($"{expectedPath}-2", actualPath);
    }
}