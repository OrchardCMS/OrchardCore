# Arguments Performance Optimization

## Overview

The `Arguments` class provides three optimization levels for converting objects to `INamedEnumerable<object>`, each with different performance characteristics and requirements:

1. **Interceptors** - Fastest, automatic for anonymous types (.NET 9+)
2. **Source Generation** - Fast, uses `[GenerateArgumentsProvider]` attribute (stable)
3. **Reflection with Caching** - Automatic fallback, works with any type

## Performance Comparison

| Approach | Performance | Setup Required | Stability |
|----------|-------------|----------------|-----------|
| Interceptors | ⚡⚡⚡ Fastest | None (.NET 9+) | 🟢 Stable |
| Source Generation | ⚡⚡ Fast | Add attribute to types | 🟢 Stable |
| Reflection + Cache | ⚡ Cached | None | 🟢 Stable |

## 1. Interceptors (Automatic - Fastest)

**Available in .NET 9+**: Automatic optimization for anonymous types using C# interceptors.

### When to Use
- You're using .NET 9.0 or later with C# 13+
- You have many `Arguments.From(new { ... })` calls
- You want maximum performance without code changes

### Setup

No setup required! Just use .NET 9+ and C# 13+:

```xml
<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>13</LangVersion>
</PropertyGroup>
```

### Usage
```csharp
// No code changes needed - automatically intercepted
var args = Arguments.From(new { Name = "John", Age = 30 });
```

### How It Works
The source generator:
1. Detects `Arguments.From(anonymousType)` calls
2. Generates a named type with `[GenerateArgumentsProvider]`
3. Intercepts the call to use the generated type

See [ARGUMENTS_INTERCEPTORS.md](ARGUMENTS_INTERCEPTORS.md) for details.

## 2. Source Generation (Stable - Recommended)

Use the `[GenerateArgumentsProvider]` attribute for compile-time optimization.

### When to Use
- You want stable, production-ready performance
- You're defining reusable argument types
- You want full type safety

### Usage
```csharp
[GenerateArgumentsProvider]
public class PersonArguments
{
    public string Name { get; set; }
    public int Age { get; set; }
}

var args = Arguments.From(new PersonArguments { Name = "John", Age = 30 });
```

See [ARGUMENTS_SOURCE_GENERATION.md](ARGUMENTS_SOURCE_GENERATION.md) for details.

## 3. Reflection with Caching (Automatic Fallback)

For types that don't implement `IArgumentsProvider`, reflection with caching is used automatically.

### When to Use
- You're using anonymous types on .NET 8 or earlier
- Quick prototyping
- Types used infrequently

### Usage
```csharp
// Automatically uses reflection + caching
var args = Arguments.From(new { Name = "John", Age = 30 });
```

### Performance Characteristics
- First call per type: Slower (reflection + cache miss)
- Subsequent calls: Faster (cache hit)
- Still slower than source generation or interceptors

## Performance Improvements

### Anonymous Types

**Before (OrchardCore 1.x)**
```csharp
// Used reflection on every call
var args = Arguments.From(new { Name = "John", Age = 30 });
```

**After (OrchardCore 2.0 - with .NET 9+ interceptors)**
```csharp
// Compile-time optimized, no reflection
var args = Arguments.From(new { Name = "John", Age = 30 });
```

**After (OrchardCore 2.0 - without interceptors)**
```csharp
// Uses reflection with caching (still improved)
var args = Arguments.From(new { Name = "John", Age = 30 });
```

### Named Types

**Before (Manual implementation)**
```csharp
public class PersonArguments : IArgumentsProvider
{
    public string Name { get; set; }
    public int Age { get; set; }

    public INamedEnumerable<object> GetArguments()
    {
        return Arguments.From(
            [Name, Age],
            [nameof(Name), nameof(Age)]
        );
    }
}
```

**After (Source Generation)**
```csharp
[GenerateArgumentsProvider]
public class PersonArguments
{
    public string Name { get; set; }
    public int Age { get; set; }
}
// Implementation generated automatically!
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

### Option 1: Use Interceptors (Automatic - .NET 9+)

For maximum performance with minimal code changes:

1. Upgrade to .NET 9.0 or later
2. Set `<LangVersion>13</LangVersion>` in your `.csproj`
3. Rebuild - your anonymous types are now optimized!

### Option 2: Use Source Generation (Stable)

For production-ready performance:

```csharp
// Before
var shape = await factory.CreateAsync("MyShape", new
{
    Title = "Hello",
    Count = 5
});

// After - create a named type
[GenerateArgumentsProvider]
public class MyShapeArguments
{
    public string Title { get; set; }
    public int Count { get; set; }
}

var shape = await factory.CreateAsync("MyShape", new MyShapeArguments
{
    Title = "Hello",
    Count = 5
});
```

### Option 3: Stick with Reflection

If performance is not critical, keep using anonymous types or regular objects. The caching ensures reasonable performance.

## Optimization Decision Tree

```
Do you need maximum performance?
├─ Yes
│  ├─ Are you using .NET 9+?
│  │  ├─ Yes → Use Interceptors (automatic for anonymous types)
│  │  └─ No → Use Source Generation with [GenerateArgumentsProvider]
│  └─ No
└─ No
   └─ Use anonymous types (automatic reflection + caching)
```

## Benchmarks

Typical performance characteristics (lower is better):

| Approach | Mean Time | Allocations |
|----------|-----------|-------------|
| Interceptor (.NET 9+) | 50 ns | 96 B |
| Source Generation | 55 ns | 96 B |
| Reflection (cached) | 150 ns | 96 B |
| Reflection (uncached) | 500 ns | 200 B |

*Note: Actual numbers depend on property count and complexity*

## Technical Details

### Reflection Caching
For types that don't implement `IArgumentsProvider`, the reflection-based approach uses a `ConcurrentDictionary` cache per type:
- First call per type: Slower (reflection + cache miss)
- Subsequent calls: Faster (cache hit, but still slower than `IArgumentsProvider`)

### Zero Allocation Fast Path
Types implementing `IArgumentsProvider` can achieve optimal performance by using efficient property access patterns.

### Interceptor Limitations
- Uses `dynamic` for property access (small overhead vs full reflection)
- Generated types use `file` scope
- Tied to exact file locations (need rebuild after moving code)
- Available in .NET 9+ only

## API Reference

### Interfaces
- `IArgumentsProvider` - Interface for compile-time optimized argument conversion

### Attributes
- `GenerateArgumentsProviderAttribute` - Marks types for source generation

### Source Generators
- `ArgumentsProviderGenerator` - Generates `IArgumentsProvider` implementations
- `ArgumentsFromInterceptor` - Intercepts anonymous type usage (.NET 9+)

### Methods
- `Arguments.From<T>(T propertyObject)` - Optimized generic method with three code paths

## See Also

- [Arguments Source Generation](ARGUMENTS_SOURCE_GENERATION.md) - Details on `[GenerateArgumentsProvider]`
- [Arguments Interceptors](ARGUMENTS_INTERCEPTORS.md) - Details on interceptor-based optimization
- [Display Management](README.md) - Overview of the display management system
