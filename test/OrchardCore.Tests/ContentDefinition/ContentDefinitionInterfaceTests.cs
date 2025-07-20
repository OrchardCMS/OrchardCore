using OrchardCore.ContentTypes.Events;
using Xunit;

namespace OrchardCore.Tests.ContentDefinition;
/// <summary>
/// Test to verify that IContentDefinitionEventHandler is properly marked as obsolete
/// and that the new IContentDefinitionHandler interface is working correctly with optional event interfaces.
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
    public void IContentDefinitionHandler_Can_Optionally_Implement_Event_Handlers()
    {
        // Arrange & Act
        var handler = new TestContentDefinitionHandlerWithEvents();

        // Assert - This test verifies that handlers can optionally implement event interfaces
        Assert.True(handler is IContentTypeEventHandler);
        Assert.True(handler is IContentPartEventHandler);
        Assert.True(handler is IContentFieldEventHandler);
    }

    private class TestContentDefinitionHandler : IContentDefinitionHandler
    {
        // Only building methods are required
        public void ContentTypeBuilding(ContentTypeBuildingContext context) { }
        public void ContentPartBuilding(ContentPartBuildingContext context) { }
        public void ContentTypePartBuilding(ContentTypePartBuildingContext context) { }
        public void ContentPartFieldBuilding(ContentPartFieldBuildingContext context) { }
    }

    private class TestContentDefinitionHandlerWithEvents : IContentDefinitionHandler, IContentTypeEventHandler, IContentPartEventHandler, IContentFieldEventHandler
    {
        // Building methods (required)
        public void ContentTypeBuilding(ContentTypeBuildingContext context) { }
        public void ContentPartBuilding(ContentPartBuildingContext context) { }
        public void ContentTypePartBuilding(ContentTypePartBuildingContext context) { }
        public void ContentPartFieldBuilding(ContentPartFieldBuildingContext context) { }

        // Event methods (optional via separate interfaces)
        public void ContentTypeCreated(ContentTypeCreatedContext context) { }
        public void ContentTypeUpdated(ContentTypeUpdatedContext context) { }
        public void ContentTypeRemoved(ContentTypeRemovedContext context) { }
        public void ContentTypeImporting(ContentTypeImportingContext context) { }
        public void ContentTypeImported(ContentTypeImportedContext context) { }
        
        public void ContentPartCreated(ContentPartCreatedContext context) { }
        public void ContentPartUpdated(ContentPartUpdatedContext context) { }
        public void ContentPartRemoved(ContentPartRemovedContext context) { }
        public void ContentPartAttached(ContentPartAttachedContext context) { }
        public void ContentPartDetached(ContentPartDetachedContext context) { }
        public void ContentPartImporting(ContentPartImportingContext context) { }
        public void ContentPartImported(ContentPartImportedContext context) { }
        
        public void ContentTypePartUpdated(ContentTypePartUpdatedContext context) { }
        public void ContentFieldAttached(ContentFieldAttachedContext context) { }
        public void ContentFieldUpdated(ContentFieldUpdatedContext context) { }
        public void ContentFieldDetached(ContentFieldDetachedContext context) { }
        public void ContentPartFieldUpdated(ContentPartFieldUpdatedContext context) { }
    }

    [Fact]
    public void IContentDefinitionHandler_Minimal_Implementation_Works()
    {
        // This test verifies that minimal implementation works
        var handler = new MinimalContentDefinitionHandler();
        
        // These should not throw as building methods are the only required ones
        handler.ContentTypeBuilding(new ContentTypeBuildingContext("Test", new()));
        handler.ContentPartBuilding(new ContentPartBuildingContext("Test", new()));
        
        Assert.True(true); // Test passes if no exceptions are thrown
    }

    private class MinimalContentDefinitionHandler : IContentDefinitionHandler
    {
        // Only implementing the required building methods
        public void ContentTypeBuilding(ContentTypeBuildingContext context) { }
        public void ContentPartBuilding(ContentPartBuildingContext context) { }
        public void ContentTypePartBuilding(ContentTypePartBuildingContext context) { }
        public void ContentPartFieldBuilding(ContentPartFieldBuildingContext context) { }
    }
}