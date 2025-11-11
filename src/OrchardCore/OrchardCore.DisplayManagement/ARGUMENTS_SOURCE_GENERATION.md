# Arguments Source Generation

This document describes how to use source-generated `IArgumentsProvider` implementations to avoid reflection overhead when converting objects to `INamedEnumerable<object>` for shape creation and other scenarios.

## Overview

The `Arguments.From<T>(T obj)` method is used throughout OrchardCore to convert objects into named argument collections for shapes. Previously, this used reflection to read object properties. Now, you can use source generation to eliminate reflection overhead and improve performance.

## Quick Start

### 1. Mark Your Class with the Attribute

```csharp
using OrchardCore.DisplayManagement;

[GenerateArgumentsProvider]
public partial class MyShapeData
{
    public string Title { get; set; }
    public int Count { get; set; }
    public bool IsActive { get; set; }
}
```

**Important**: The class must be marked as `partial` for source generation to work.

### 2. Use It With Arguments.From

```csharp
var data = new MyShapeData
{
    Title = "Hello World",
    Count = 42,
    IsActive = true
};

// This now uses generated code - zero reflection!
var args = Arguments.From(data);

// Or use directly with shape factory
var shape = await shapeFactory.CreateAsync("MyShape", data);
```

## Performance Benefits

### Before (Reflection-based)

```csharp
// Uses reflection on every call - slower
var shape = await factory.CreateAsync("MyShape", new
{
    Title = "Test",
    Count = 5,
    IsActive = true
});
```

- **First Call**: Reflection + caching overhead
- **Subsequent Calls**: Cached reflection (still slower than no reflection)
- **Memory**: Cached reflection data per type

### After (Source Generated)

```csharp
[GenerateArgumentsProvider]
public partial class MyShapeData
{
    public string Title { get; set; }
    public int Count { get; set; }
    public bool IsActive { get; set; }
}

var data = new MyShapeData { Title = "Test", Count = 5, IsActive = true };
var shape = await factory.CreateAsync("MyShape", data);
```

- **All Calls**: Direct property access via generated code
- **Memory**: No caching needed
- **Performance**: ~10-50x faster for property extraction

## Real-World Examples

OrchardCore includes several source-generated models that you can use as references:

### Example 1: Admin Navigation Data

See: `OrchardCore.Admin.Models.AdminNavigationShapeData`

```csharp
using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin.Models;

[GenerateArgumentsProvider]
public partial class AdminNavigationShapeData
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string IconClass { get; set; }
    public int Position { get; set; }
    public bool IsActive { get; set; }
}
```

### Example 2: User Registration Email

See: `OrchardCore.Users.Models.UserRegistrationEmailModel`

```csharp
using OrchardCore.DisplayManagement;

namespace OrchardCore.Users.Models;

[GenerateArgumentsProvider]
public partial class UserRegistrationEmailModel
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string ConfirmationUrl { get; set; }
    public string SiteName { get; set; }
}
```

### Example 3: Cache Shape Metadata

See: `OrchardCore.DynamicCache.Models.CacheShapeMetadata`

```csharp
using OrchardCore.DisplayManagement;

namespace OrchardCore.DynamicCache.Models;

[GenerateArgumentsProvider]
public partial class CacheShapeMetadata
{
    public string CacheId { get; set; }
    public string CacheTag { get; set; }
    public string CacheContext { get; set; }
    public string CacheDuration { get; set; }
    public bool VaryByQueryString { get; set; }
    public bool VaryByRoute { get; set; }
}
```

### Example 4: Content Item Summary

See: `OrchardCore.ContentManagement.Display.Models.ContentItemSummaryModel`

```csharp
using OrchardCore.DisplayManagement;

namespace OrchardCore.ContentManagement.Display.Models;

[GenerateArgumentsProvider]
public partial class ContentItemSummaryModel
{
    public string ContentItemId { get; set; }
    public string DisplayText { get; set; }
    public string ContentType { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public DateTime? ModifiedUtc { get; set; }
    public DateTime? PublishedUtc { get; set; }
    public string Owner { get; set; }
    public string Author { get; set; }
    public bool IsPublished { get; set; }
    public bool HasDraft { get; set; }
}
```

## Usage in Display Drivers

Here's how you might use a source-generated model in a display driver:

```csharp
public class MyDisplayDriver : DisplayDriver<ContentItem>
{
    public override IDisplayResult Display(ContentItem model, BuildDisplayContext context)
    {
        var summaryModel = new ContentItemSummaryModel
        {
            ContentItemId = model.ContentItemId,
            DisplayText = model.DisplayText,
            ContentType = model.ContentType,
            CreatedUtc = model.CreatedUtc,
            ModifiedUtc = model.ModifiedUtc,
            PublishedUtc = model.PublishedUtc,
            Owner = model.Owner,
            Author = model.Author,
            IsPublished = model.Published,
            HasDraft = model.Latest && !model.Published
        };

        return Initialize<IShape>("ContentItemSummary", shape =>
        {
            // This uses the source-generated GetArguments() method - zero reflection!
            var args = Arguments.From(summaryModel);
            
            foreach (var prop in args.Named)
            {
                shape.Properties[prop.Key] = prop.Value;
            }
        });
    }
}
```

## Advanced Usage

### Manual Implementation

If you need custom logic when converting to arguments, you can manually implement `IArgumentsProvider`:

```csharp
public class CustomDataModel : IArgumentsProvider
{
    public string Name { get; set; }
    public int Value { get; set; }

    public INamedEnumerable<object> GetArguments()
    {
        // Custom logic - transform values before returning
        var upperName = Name?.ToUpperInvariant() ?? "DEFAULT";
        var doubledValue = Value * 2;

        return ArgumentsProviderHelper.Create(
            [upperName, doubledValue],
            [nameof(Name), nameof(Value)]
        );
    }
}
```

### When to Use Source Generation

✅ **DO use source generation when:**
- Creating shapes with data models in drivers
- Frequently converting objects to arguments
- Working with known, named types
- Performance is important

❌ **DON'T use source generation when:**
- Using anonymous objects (these still use cached reflection)
- One-off scenarios where performance doesn't matter
- The type structure changes frequently during development

### Anonymous Objects Still Work

Anonymous objects continue to use the cached reflection approach:

```csharp
// This still works - uses cached reflection
var shape = await factory.CreateAsync("MyShape", new
{
    Title = "Test",
    Count = 5
});
```

This is fine for:
- Quick prototyping
- One-off shapes
- Non-performance-critical paths

## What Gets Generated

When you mark a class with `[GenerateArgumentsProvider]`, the source generator creates a partial class implementation of `IArgumentsProvider`:

```csharp
// Your code:
[GenerateArgumentsProvider]
public partial class MyData
{
    public string Name { get; set; }
    public int Value { get; set; }
}

// Generated code (automatic):
public partial class MyData : OrchardCore.DisplayManagement.IArgumentsProvider
{
    OrchardCore.DisplayManagement.INamedEnumerable<object> 
        OrchardCore.DisplayManagement.IArgumentsProvider.GetArguments()
    {
        var values = new object[2];
        var names = new string[2];

        values[0] = Name;
        names[0] = "Name";
        values[1] = Value;
        names[1] = "Value";

        return OrchardCore.DisplayManagement.Arguments.From(values, names);
    }
}
```

## Requirements

- The class must be declared as `partial`
- The class must have at least one public instance property
- All public instance properties with getters will be included

## Migration Guide

### Step 1: Identify High-Traffic Code

Look for:
- Display drivers that create many shapes
- Services that frequently call `Arguments.From`
- Hot paths in your profiling data

### Step 2: Create Model Classes

Replace anonymous objects with named partial classes:

```csharp
// Before:
return await factory.CreateAsync("MyShape", new { Title = title, Count = count });

// After:
[GenerateArgumentsProvider]
public partial class MyShapeData
{
    public string Title { get; set; }
    public int Count { get; set; }
}

var data = new MyShapeData { Title = title, Count = count };
return await factory.CreateAsync("MyShape", data);
```

### Step 3: Test

The behavior should be identical, just faster. Your existing tests should pass without changes.

## Troubleshooting

### "Type must be partial"

**Error**: The type must be declared as partial to use source generation.

**Solution**: Add the `partial` keyword to your class declaration:

```csharp
// Wrong:
[GenerateArgumentsProvider]
public class MyData { ... }

// Correct:
[GenerateArgumentsProvider]
public partial class MyData { ... }
```

### "No properties found"

**Warning**: The generator won't create an implementation if there are no public instance properties.

**Solution**: Ensure your class has at least one public property with a getter.

### Generated code not found

**Issue**: IntelliSense doesn't show the `IArgumentsProvider` implementation.

**Solution**:
1. Rebuild your project
2. Close and reopen files
3. Restart Visual Studio if needed
4. Check that the source generator project reference is correct

## Best Practices

1. **Use descriptive names**: `UserRegistrationEmailData` is better than `Data`
2. **Group related properties**: Create focused models for specific scenarios
3. **Keep models simple**: Avoid complex logic in getters
4. **Document usage**: Add XML comments explaining what the model is for
5. **Test performance**: Measure the impact in your specific scenarios
6. **Look at existing examples**: Check the models in OrchardCore modules for reference

## See Also

- [Arguments.cs](./Arguments.cs) - Core Arguments class
- [IArgumentsProvider.cs](./IArgumentsProvider.cs) - Provider interface
- [ArgumentsProviderGenerator.cs](../OrchardCore.SourceGenerators/ArgumentsProviderGenerator.cs) - Source generator implementation
- OrchardCore module models - Real-world usage examples
