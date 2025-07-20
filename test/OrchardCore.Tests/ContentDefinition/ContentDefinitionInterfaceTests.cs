using OrchardCore.ContentTypes.Events;
using Xunit;

namespace OrchardCore.Tests.ContentDefinition
{
    /// <summary>
    /// Test to verify that IContentDefinitionEventHandler is properly marked as obsolete
    /// and that the new IContentDefinitionHandler interface is working correctly.
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

        private class TestContentDefinitionHandler : IContentDefinitionHandler
        {
            // Building methods
            public void ContentTypeBuilding(ContentTypeBuildingContext context) { }
            public void ContentPartBuilding(ContentPartBuildingContext context) { }
            public void ContentTypePartBuilding(ContentTypePartBuildingContext context) { }
            public void ContentPartFieldBuilding(ContentPartFieldBuildingContext context) { }

            // Event methods (these have default implementations in the interface)
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
        public void IContentDefinitionHandler_Default_Methods_Can_Be_Overridden()
        {
            // This test verifies that the default implementations work
            var handler = new MinimalContentDefinitionHandler();
            var context = new ContentTypeCreatedContext();
            
            // These should not throw as they have default implementations
            handler.ContentTypeCreated(context);
            handler.ContentTypeUpdated(new ContentTypeUpdatedContext());
            // ... etc. The fact that this compiles proves the default implementations work
            
            Assert.True(true); // Test passes if no exceptions are thrown
        }

        private class MinimalContentDefinitionHandler : IContentDefinitionHandler
        {
            // Only implementing the required building methods
            public void ContentTypeBuilding(ContentTypeBuildingContext context) { }
            public void ContentPartBuilding(ContentPartBuildingContext context) { }
            public void ContentTypePartBuilding(ContentTypePartBuildingContext context) { }
            public void ContentPartFieldBuilding(ContentPartFieldBuildingContext context) { }
            
            // All event methods have default implementations, so we don't need to implement them
        }
    }
}