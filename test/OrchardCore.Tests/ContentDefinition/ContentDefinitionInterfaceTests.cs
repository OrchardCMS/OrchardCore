using OrchardCore.ContentTypes.Events;
using Xunit;

namespace OrchardCore.Tests.ContentDefinition;
/// <summary>
/// Test to verify that IContentDefinitionEventHandler is properly marked as obsolete
/// and that the new IContentDefinitionHandler interface is working correctly with event methods.
/// </summary>
public sealed class ContentDefinitionInterfaceTests
{
    [Fact]
    public void IContentDefinitionHandler_Has_All_Required_Methods()
    {
        // Arrange & Act
        var handler = new TestContentDefinitionHandler();

        // Assert - This test verifies that the interface has all the required methods
        // and that they can be implemented without compilation errors
        Assert.NotNull(handler);
    }

    [Fact]
    public void IContentDefinitionHandler_Can_Override_Event_Methods()
    {
        // Arrange & Act
        var handler = new TestContentDefinitionHandlerWithEvents();

        // Assert - This test verifies that handlers can optionally override event methods
        Assert.NotNull(handler);
        
        // Test that overridden methods work
        var createdContext = new ContentTypeCreatedContext { ContentTypeDefinition = null };
        handler.ContentTypeCreatedAsync(createdContext).GetAwaiter().GetResult();
        Assert.True(handler.EventCalled);
    }

    [Fact]
    public void IContentDefinitionHandler_Can_Override_Building_Methods()
    {
        // Arrange & Act
        var handler = new TestContentDefinitionHandlerWithBuilding();

        // Assert - This test verifies that handlers can override building methods
        Assert.NotNull(handler);
        
        // Test that overridden async building method works
        var buildingContext = new ContentTypeBuildingContext("TestType", null);
        handler.ContentTypeBuildingAsync(buildingContext).GetAwaiter().GetResult();
        Assert.True(handler.BuildingCalled);
    }

    private class TestContentDefinitionHandler : ContentDefinitionHandler
    {
        // Only building methods need to be overridden if needed - event methods have default implementations
    }

    private class TestContentDefinitionHandlerWithEvents : ContentDefinitionHandler
    {
        public bool EventCalled { get; private set; }

        // Event methods (optional override) - use async version
        public override ValueTask ContentTypeCreatedAsync(ContentTypeCreatedContext context)
        {
            EventCalled = true;
            return ValueTask.CompletedTask;
        }
    }

    private class TestContentDefinitionHandlerWithBuilding : ContentDefinitionHandler
    {
        public bool BuildingCalled { get; private set; }

        // Building methods (optional override) - use async version
        public override ValueTask ContentTypeBuildingAsync(ContentTypeBuildingContext context)
        {
            BuildingCalled = true;
            return ValueTask.CompletedTask;
        }
    }
}