# Migrating from IContentDefinitionEventHandler to IContentDefinitionHandler

As of Orchard Core 3.0, the `IContentDefinitionEventHandler` interface has been marked as obsolete in favor of using the unified `IContentDefinitionHandler` interface. This guide will help you migrate your existing event handlers.

## Background

Previously, content definition events were triggered through two separate interfaces:

- `IContentDefinitionEventHandler` - for lifecycle events (created, updated, removed, etc.)
- `IContentDefinitionHandler` - for building events (content type building, part building, etc.)

This separation led to confusion and inconsistent event handling patterns. To improve the architecture, all content definition events have been consolidated into the `IContentDefinitionHandler` interface.

## What Changed

1. **IContentDefinitionEventHandler is obsolete**: The interface is marked with `[Obsolete]` attribute and will be removed in a future version.

2. **IContentDefinitionHandler is extended**: All event methods from `IContentDefinitionEventHandler` have been added to `IContentDefinitionHandler` with default implementations.

3. **Event triggering moved**: Events are now triggered by `ContentDefinitionManager` instead of `ContentDefinitionService`.

4. **Backward compatibility maintained**: During the transition period, both interfaces continue to work.

## Migration Steps

### Step 1: Update Interface Implementation

Change your class to implement `IContentDefinitionHandler` instead of `IContentDefinitionEventHandler`:

```csharp
// Before
public class MyContentHandler : IContentDefinitionEventHandler
{
    public void ContentTypeCreated(ContentTypeCreatedContext context) 
    {
        // Your logic
    }
    
    public void ContentTypeUpdated(ContentTypeUpdatedContext context) { }
    public void ContentTypeRemoved(ContentTypeRemovedContext context) { }
    // ... implement all required methods
}

// After
public class MyContentHandler : IContentDefinitionHandler
{
    // Building methods (required)
    public void ContentTypeBuilding(ContentTypeBuildingContext context) { }
    public void ContentPartBuilding(ContentPartBuildingContext context) { }
    public void ContentTypePartBuilding(ContentTypePartBuildingContext context) { }
    public void ContentPartFieldBuilding(ContentPartFieldBuildingContext context) { }
    
    // Event methods (optional - override only what you need)
    public void ContentTypeCreated(ContentTypeCreatedContext context) 
    {
        // Your logic
    }
    
    // Other event methods have default implementations
}
```

### Step 2: Update Service Registration

Update your service registration in `Startup.cs`:

```csharp
// Before
services.AddScoped<IContentDefinitionEventHandler, MyContentHandler>();

// After
services.AddScoped<IContentDefinitionHandler, MyContentHandler>();
```

### Step 3: Remove Obsolete Interface (Optional)

If you want to completely remove the old interface, update your using statements:

```csharp
// Remove this using if no longer needed
// using OrchardCore.ContentTypes.Events.IContentDefinitionEventHandler;

// This should already be present
using OrchardCore.ContentTypes.Events;
```

## Benefits of Migration

1. **Unified Interface**: Single interface for all content definition handling
2. **Simplified Implementation**: Event methods have default implementations
3. **Better Architecture**: Events triggered by appropriate service (`ContentDefinitionManager`)
4. **Future-Proof**: Prepares your code for future Orchard Core versions

## Backward Compatibility

During the transition period:

- Both `IContentDefinitionEventHandler` and `IContentDefinitionHandler` registrations work
- Events are triggered through both interfaces
- No immediate breaking changes to existing code

However, you should migrate as soon as possible since `IContentDefinitionEventHandler` will be removed in a future version.

## Example: Complete Migration

Here's a complete example showing the migration of a cache invalidation handler:

```csharp
// Before - IContentDefinitionEventHandler
public class CacheInvalidationHandler : IContentDefinitionEventHandler
{
    private readonly IMemoryCache _memoryCache;

    public CacheInvalidationHandler(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public void ContentTypeCreated(ContentTypeCreatedContext context) => InvalidateCache();
    public void ContentTypeUpdated(ContentTypeUpdatedContext context) => InvalidateCache();
    public void ContentTypeRemoved(ContentTypeRemovedContext context) => InvalidateCache();
    
    // All other methods with empty implementations
    public void ContentTypeImporting(ContentTypeImportingContext context) { }
    public void ContentTypeImported(ContentTypeImportedContext context) { }
    // ... many more empty methods

    private void InvalidateCache() => _memoryCache.Remove("my-cache-key");
}

// After - IContentDefinitionHandler
public class CacheInvalidationHandler : IContentDefinitionHandler
{
    private readonly IMemoryCache _memoryCache;

    public CacheInvalidationHandler(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    // Required building methods (empty implementations)
    public void ContentTypeBuilding(ContentTypeBuildingContext context) { }
    public void ContentPartBuilding(ContentPartBuildingContext context) { }
    public void ContentTypePartBuilding(ContentTypePartBuildingContext context) { }
    public void ContentPartFieldBuilding(ContentPartFieldBuildingContext context) { }

    // Event methods (override only what's needed)
    public void ContentTypeCreated(ContentTypeCreatedContext context) => InvalidateCache();
    public void ContentTypeUpdated(ContentTypeUpdatedContext context) => InvalidateCache();
    public void ContentTypeRemoved(ContentTypeRemovedContext context) => InvalidateCache();

    // All other event methods have default implementations - no need to implement them!

    private void InvalidateCache() => _memoryCache.Remove("my-cache-key");
}
```

## Troubleshooting

### Compilation Errors

If you get compilation errors after migration:

1. Make sure you're implementing all required building methods
2. Check that you have the correct using statements
3. Verify your service registration is updated

### Events Not Firing

If your events are not being triggered:

1. Ensure you've registered your service as `IContentDefinitionHandler`
2. Check that you've implemented the building methods (they are required)
3. Verify that the events you're handling are actually being triggered by the operations you're testing

### Multiple Registrations

During the transition period, you might accidentally register both interfaces. This is harmless but redundant:

```csharp
// Avoid this during migration
services.AddScoped<IContentDefinitionEventHandler, MyHandler>();
services.AddScoped<IContentDefinitionHandler, MyHandler>(); // Redundant
```

Choose one registration method:

```csharp
// Preferred approach
services.AddScoped<IContentDefinitionHandler, MyHandler>();
```