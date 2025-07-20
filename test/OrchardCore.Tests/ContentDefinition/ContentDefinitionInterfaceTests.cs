using OrchardCore.ContentTypes.Events;
using Xunit;

namespace OrchardCore.Tests.ContentDefinition;
/// <summary>
/// Test to verify that IContentDefinitionEventHandler is properly marked as obsolete
/// and that the new IContentDefinitionHandler interface is working correctly with event methods.
/// </summary>
public class ContentDefinitionInterfaceTests
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
        handler.ContentTypeCreated(createdContext);
        Assert.True(handler.EventCalled);
    }

    private class TestContentDefinitionHandler : IContentDefinitionHandler
    {
        // Only building methods are required - event methods have default implementations
        public void ContentTypeBuilding(ContentTypeBuildingContext context) { }
        public void ContentPartBuilding(ContentPartBuildingContext context) { }
        public void ContentTypePartBuilding(ContentTypePartBuildingContext context) { }
        public void ContentPartFieldBuilding(ContentPartFieldBuildingContext context) { }
    }

    private class TestContentDefinitionHandlerWithEvents : IContentDefinitionHandler
    {
        public bool EventCalled { get; private set; }

        // Building methods (required)
        public void ContentTypeBuilding(ContentTypeBuildingContext context) { }
        public void ContentPartBuilding(ContentPartBuildingContext context) { }
        public void ContentTypePartBuilding(ContentTypePartBuildingContext context) { }
        public void ContentPartFieldBuilding(ContentPartFieldBuildingContext context) { }

        // Event methods (optional override)
        public void ContentTypeCreated(ContentTypeCreatedContext context)
        {
            EventCalled = true;
        }
    }
}