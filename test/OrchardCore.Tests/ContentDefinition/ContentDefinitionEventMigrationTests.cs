using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Events;
using OrchardCore.ContentTypes.Services;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.Modules;
using Xunit;

namespace OrchardCore.Tests.ContentDefinition
{
    public class ContentDefinitionEventMigrationTests
    {
        [Fact]
        public async Task ContentTypeCreated_Event_Is_Triggered_Through_Both_Old_And_New_Interfaces()
        {
            // Arrange
            var services = new ServiceCollection();
            
            var mockContentDefinitionManager = new MockContentDefinitionManager();
            var oldEventHandler = new MockContentDefinitionEventHandler();
            var newHandler = new MockContentDefinitionHandler();
            
            services.AddSingleton<IContentDefinitionManager>(mockContentDefinitionManager);
            services.AddSingleton<IContentDefinitionEventHandler>(oldEventHandler);
            services.AddSingleton<IContentDefinitionHandler>(newHandler);
            services.AddSingleton<ILogger<IContentDefinitionService>>(NullLogger<IContentDefinitionService>.Instance);
            services.AddSingleton<IStringLocalizer<ContentDefinitionService>>(new MockStringLocalizer());
            services.AddSingleton<IOptions<ContentOptions>>(Options.Create(new ContentOptions()));
            
            var serviceProvider = services.BuildServiceProvider();
            var contentDefinitionService = new ContentDefinitionService(
                serviceProvider.GetService<IContentDefinitionManager>(),
                serviceProvider.GetServices<IContentDefinitionEventHandler>(),
                Array.Empty<ContentPart>(),
                Array.Empty<ContentField>(),
                serviceProvider.GetService<IOptions<ContentOptions>>(),
                serviceProvider.GetService<ILogger<IContentDefinitionService>>(),
                serviceProvider.GetService<IStringLocalizer<ContentDefinitionService>>()
            );

            // Act
            await contentDefinitionService.AddTypeAsync("TestType", "Test Type");

            // Assert
            Assert.True(oldEventHandler.ContentTypeCreatedCalled, "Old event handler should have been called");
            Assert.True(newHandler.ContentTypeCreatedCalled, "New handler should have been called");
            Assert.True(mockContentDefinitionManager.TriggerContentTypeCreatedCalled, "ContentDefinitionManager trigger should have been called");
        }

        private class MockContentDefinitionManager : IContentDefinitionManager
        {
            public bool TriggerContentTypeCreatedCalled { get; private set; }
            
            public Task<IEnumerable<ContentTypeDefinition>> LoadTypeDefinitionsAsync() => Task.FromResult(Enumerable.Empty<ContentTypeDefinition>());
            public Task<IEnumerable<ContentTypeDefinition>> ListTypeDefinitionsAsync() => Task.FromResult(Enumerable.Empty<ContentTypeDefinition>());
            public Task<IEnumerable<ContentPartDefinition>> LoadPartDefinitionsAsync() => Task.FromResult(Enumerable.Empty<ContentPartDefinition>());
            public Task<IEnumerable<ContentPartDefinition>> ListPartDefinitionsAsync() => Task.FromResult(Enumerable.Empty<ContentPartDefinition>());
            public Task<ContentTypeDefinition> LoadTypeDefinitionAsync(string name) => Task.FromResult<ContentTypeDefinition>(null);
            public Task<ContentTypeDefinition> GetTypeDefinitionAsync(string name) => Task.FromResult<ContentTypeDefinition>(null);
            public Task<ContentPartDefinition> LoadPartDefinitionAsync(string name) => Task.FromResult<ContentPartDefinition>(null);
            public Task<ContentPartDefinition> GetPartDefinitionAsync(string name) => Task.FromResult<ContentPartDefinition>(null);
            public Task DeleteTypeDefinitionAsync(string name) => Task.CompletedTask;
            public Task DeletePartDefinitionAsync(string name) => Task.CompletedTask;
            public Task StoreTypeDefinitionAsync(ContentTypeDefinition contentTypeDefinition) => Task.CompletedTask;
            public Task StorePartDefinitionAsync(ContentPartDefinition contentPartDefinition) => Task.CompletedTask;
            public Task<string> GetIdentifierAsync() => Task.FromResult("test-identifier");

            public void TriggerContentTypeCreated(ContentTypeCreatedContext context)
            {
                TriggerContentTypeCreatedCalled = true;
            }

            public void TriggerContentTypeUpdated(ContentTypeUpdatedContext context) { }
            public void TriggerContentTypeRemoved(ContentTypeRemovedContext context) { }
            public void TriggerContentTypeImporting(ContentTypeImportingContext context) { }
            public void TriggerContentTypeImported(ContentTypeImportedContext context) { }
            public void TriggerContentPartCreated(ContentPartCreatedContext context) { }
            public void TriggerContentPartUpdated(ContentPartUpdatedContext context) { }
            public void TriggerContentPartRemoved(ContentPartRemovedContext context) { }
            public void TriggerContentPartAttached(ContentPartAttachedContext context) { }
            public void TriggerContentPartDetached(ContentPartDetachedContext context) { }
            public void TriggerContentPartImporting(ContentPartImportingContext context) { }
            public void TriggerContentPartImported(ContentPartImportedContext context) { }
            public void TriggerContentTypePartUpdated(ContentTypePartUpdatedContext context) { }
            public void TriggerContentFieldAttached(ContentFieldAttachedContext context) { }
            public void TriggerContentFieldUpdated(ContentFieldUpdatedContext context) { }
            public void TriggerContentFieldDetached(ContentFieldDetachedContext context) { }
            public void TriggerContentPartFieldUpdated(ContentPartFieldUpdatedContext context) { }
        }

        private class MockContentDefinitionEventHandler : IContentDefinitionEventHandler
        {
            public bool ContentTypeCreatedCalled { get; private set; }

            public void ContentTypeCreated(ContentTypeCreatedContext context)
            {
                ContentTypeCreatedCalled = true;
            }

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

        private class MockContentDefinitionHandler : IContentDefinitionHandler
        {
            public bool ContentTypeCreatedCalled { get; private set; }

            public void ContentTypeBuilding(ContentTypeBuildingContext context) { }
            public void ContentPartBuilding(ContentPartBuildingContext context) { }
            public void ContentTypePartBuilding(ContentTypePartBuildingContext context) { }
            public void ContentPartFieldBuilding(ContentPartFieldBuildingContext context) { }

            public void ContentTypeCreated(ContentTypeCreatedContext context)
            {
                ContentTypeCreatedCalled = true;
            }

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

        private class MockStringLocalizer : IStringLocalizer<ContentDefinitionService>
        {
            public LocalizedString this[string name] => new(name, name);
            public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));
            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Enumerable.Empty<LocalizedString>();
        }
    }
}