# Arguments Performance Optimization

## Overview

The `Arguments` class has been optimized to reduce reflection overhead when converting objects to `INamedEnumerable<object>`. This is achieved through the introduction of the `IArgumentsProvider` interface.

## Performance Improvements

### Before (Reflection-based)
```csharp
// This used reflection on every call (cached per type but still slow)
var args = Arguments.From(new { Name = "John", Age = 30 });
```

### After (Interface-based)
```csharp
// Fast path - no reflection
public class PersonArguments : IArgumentsProvider
{
    public string Name { get; set; }
    public int Age { get; set; }

    public INamedEnumerable<object> GetArguments()
    {
        return ArgumentsProviderHelper.Create(
            [Name, Age],
            [nameof(Name), nameof(Age)]
        );
    }
}

var args = Arguments.From(new PersonArguments { Name = "John", Age = 30 });
```

## Usage

### For Anonymous Types (Still Uses Reflection)
Anonymous types continue to work as before, using cached reflection:

```csharp
// This automatically falls back to reflection
var shape = await factory.CreateAsync("MyShape", new { Title = "Hello", Count = 5 });
```

### For Named Types (Recommended - No Reflection)
For better performance with named types, implement `IArgumentsProvider`:

```csharp
public class MyShapeModel : IArgumentsProvider
{
    public string Title { get; set; }
    public int Count { get; set; }

    public INamedEnumerable<object> GetArguments()
    {
        return ArgumentsProviderHelper.Create(
            [Title, Count],
            [nameof(Title), nameof(Count)]
        );
    }
}

var model = new MyShapeModel { Title = "Hello", Count = 5 };
var shape = await factory.CreateAsync("MyShape", model);
```

### Using the Attribute (For Future Source Generation)
Mark your types with `[GenerateArgumentsProvider]` to enable automatic generation in the future:

```csharp
[GenerateArgumentsProvider]
public partial class MyShapeModel
{
    public string Title { get; set; }
    public int Count { get; set; }
}

// When source generation is implemented, the IArgumentsProvider implementation
// will be automatically generated at compile time
```

## Migration Guide

### Existing Code
No changes required! All existing code continues to work:

```csharp
// These all continue to work exactly as before:
Arguments.From(new { Foo = 1, Bar = "test" });
Arguments.From(dictionary);
Arguments.From(values, names);
```

### Optimization Opportunities
For frequently-called code paths, consider implementing `IArgumentsProvider`:

```csharp
// Before (uses reflection)
public class MyDriver : DisplayDriver<MyPart>
{
    public override Task<IDisplayResult> DisplayAsync(MyPart part, BuildPartDisplayContext context)
    {
        return Task.FromResult<IDisplayResult>(
            Initialize<MyViewModel>("MyShape", model =>
            {
                model.Title = part.Title;
            })
        );
    }
}

// After (no reflection needed if MyViewModel implements IArgumentsProvider)
public class MyViewModel : IArgumentsProvider
{
    public string Title { get; set; }

    public INamedEnumerable<object> GetArguments()
    {
        return ArgumentsProviderHelper.Create([Title], [nameof(Title)]);
    }
}
```

## Technical Details

### Reflection Caching
For types that don't implement `IArgumentsProvider`, the reflection-based approach is still available and uses a `ConcurrentDictionary` cache per type. This means:
- First call per type: Slower (reflection + cache miss)
- Subsequent calls: Faster (cache hit, but still slower than `IArgumentsProvider`)

### Zero Allocation Fast Path
Types implementing `IArgumentsProvider` can achieve zero-allocation property access by using array pooling or pre-allocated arrays in the `GetArguments()` implementation.

## Future Enhancements

A source generator can be added to automatically implement `IArgumentsProvider` for types marked with `[GenerateArgumentsProvider]`, providing the best of both worlds:
- Compile-time code generation
- No manual implementation needed
- Maximum performance

## API Reference

### New Types
- `IArgumentsProvider` - Interface for providing compile-time optimized argument conversion
- `GenerateArgumentsProviderAttribute` - Attribute to mark types for source generation
- `ArgumentsProviderHelper` - Helper methods for creating implementations
- `ArgumentsExtensions` - Extension methods for convenient usage

### Modified Methods
- `Arguments.From<T>(T propertyObject)` - Now generic with constraint and optimized path for `IArgumentsProvider`

### Internal Changes
- `ArgumentsReflectionHelper` - New internal helper for reflection fallback
- Removed `_propertiesAccessors` field from `Arguments` class
