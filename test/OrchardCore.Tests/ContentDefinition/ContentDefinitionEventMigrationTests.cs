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

namespace OrchardCore.Tests.ContentDefinition;

public class ContentDefinitionEventMigrationTests
{
    [Fact]
    public async Task ContentTypeCreated_Event_Is_Triggered_Through_Both_Old_And_New_Interfaces()
    {
        // Arrange
        var services = new ServiceCollection();
        
        var oldEventHandler = new MockContentDefinitionEventHandler();
        var newHandler = new MockContentDefinitionHandler();
        var mockContentDefinitionManager = new MockContentDefinitionManager(
            new[] { oldEventHandler }, 
            new[] { newHandler }
        );
        
        services.AddSingleton<IContentDefinitionManager>(mockContentDefinitionManager);
        services.AddSingleton<ILogger<IContentDefinitionService>>(NullLogger<IContentDefinitionService>.Instance);
        services.AddSingleton<IStringLocalizer<ContentDefinitionService>>(new MockStringLocalizer());
        services.AddSingleton<IOptions<ContentOptions>>(Options.Create(new ContentOptions()));
        
        var serviceProvider = services.BuildServiceProvider();
        var contentDefinitionService = new ContentDefinitionService(
            serviceProvider.GetService<IContentDefinitionManager>(),
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
    }

    private class MockContentDefinitionManager : IContentDefinitionManager
    {
        private readonly IEnumerable<IContentDefinitionEventHandler> _eventHandlers;
        private readonly IEnumerable<IContentDefinitionHandler> _handlers;

        public MockContentDefinitionManager()
        {
            _eventHandlers = new List<IContentDefinitionEventHandler>();
            _handlers = new List<IContentDefinitionHandler>();
        }

        public MockContentDefinitionManager(IEnumerable<IContentDefinitionEventHandler> eventHandlers, IEnumerable<IContentDefinitionHandler> handlers)
        {
            _eventHandlers = eventHandlers;
            _handlers = handlers;
        }

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
            foreach (var handler in _handlers)
            {
                handler.ContentTypeCreated(context);
            }
            foreach (var eventHandler in _eventHandlers)
            {
                eventHandler.ContentTypeCreated(context);
            }
        }

        public void TriggerContentTypeUpdated(ContentTypeUpdatedContext context)
        {
            foreach (var handler in _handlers)
            {
                handler.ContentTypeUpdated(context);
            }
            foreach (var eventHandler in _eventHandlers)
            {
                eventHandler.ContentTypeUpdated(context);
            }
        }

        public void TriggerContentTypeRemoved(ContentTypeRemovedContext context)
        {
            foreach (var handler in _handlers)
            {
                handler.ContentTypeRemoved(context);
            }
            foreach (var eventHandler in _eventHandlers)
            {
                eventHandler.ContentTypeRemoved(context);
            }
        }

        public void TriggerContentPartCreated(ContentPartCreatedContext context)
        {
            foreach (var handler in _handlers)
            {
                handler.ContentPartCreated(context);
            }
            foreach (var eventHandler in _eventHandlers)
            {
                eventHandler.ContentPartCreated(context);
            }
        }

        public void TriggerContentPartUpdated(ContentPartUpdatedContext context)
        {
            foreach (var handler in _handlers)
            {
                handler.ContentPartUpdated(context);
            }
            foreach (var eventHandler in _eventHandlers)
            {
                eventHandler.ContentPartUpdated(context);
            }
        }

        public void TriggerContentPartRemoved(ContentPartRemovedContext context)
        {
            foreach (var handler in _handlers)
            {
                handler.ContentPartRemoved(context);
            }
            foreach (var eventHandler in _eventHandlers)
            {
                eventHandler.ContentPartRemoved(context);
            }
        }

        public void TriggerContentPartAttached(ContentPartAttachedContext context)
        {
            foreach (var handler in _handlers)
            {
                handler.ContentPartAttached(context);
            }
            foreach (var eventHandler in _eventHandlers)
            {
                eventHandler.ContentPartAttached(context);
            }
        }

        public void TriggerContentPartDetached(ContentPartDetachedContext context)
        {
            foreach (var handler in _handlers)
            {
                handler.ContentPartDetached(context);
            }
            foreach (var eventHandler in _eventHandlers)
            {
                eventHandler.ContentPartDetached(context);
            }
        }

        public void TriggerContentTypePartUpdated(ContentTypePartUpdatedContext context)
        {
            foreach (var handler in _handlers)
            {
                handler.ContentTypePartUpdated(context);
            }
            foreach (var eventHandler in _eventHandlers)
            {
                eventHandler.ContentTypePartUpdated(context);
            }
        }

        public void TriggerContentFieldAttached(ContentFieldAttachedContext context)
        {
            foreach (var handler in _handlers)
            {
                handler.ContentFieldAttached(context);
            }
            foreach (var eventHandler in _eventHandlers)
            {
                eventHandler.ContentFieldAttached(context);
            }
        }

        public void TriggerContentFieldDetached(ContentFieldDetachedContext context)
        {
            foreach (var handler in _handlers)
            {
                handler.ContentFieldDetached(context);
            }
            foreach (var eventHandler in _eventHandlers)
            {
                eventHandler.ContentFieldDetached(context);
            }
        }

        public void TriggerContentPartFieldUpdated(ContentPartFieldUpdatedContext context)
        {
            foreach (var handler in _handlers)
            {
                handler.ContentPartFieldUpdated(context);
            }
            foreach (var eventHandler in _eventHandlers)
            {
                eventHandler.ContentPartFieldUpdated(context);
            }
        }
    }

    private sealed class MockContentDefinitionEventHandler : IContentDefinitionEventHandler
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

    private sealed class MockContentDefinitionHandler : ContentDefinitionHandler
    {
        public bool ContentTypeCreatedCalled { get; private set; }

        public override void ContentTypeCreated(ContentTypeCreatedContext context)
        {
            ContentTypeCreatedCalled = true;
        }
    }

    private sealed class MockStringLocalizer : IStringLocalizer<ContentDefinitionService>
    {
        public LocalizedString this[string name] => new(name, name);
        public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Enumerable.Empty<LocalizedString>();
    }
}