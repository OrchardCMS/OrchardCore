# OrchardCore Source Generators

OrchardCore includes source generators that optimize performance by generating code at compile time, eliminating reflection overhead in critical paths.

## Arguments Source Generation

### Overview

The `Arguments` class is used throughout OrchardCore to convert objects into named argument collections for shapes and other scenarios. Source generation provides zero-reflection property access for optimal performance.

### Recommended: Use Named Types with `[GenerateArgumentsProvider]`

**This is the preferred approach** for production code. Mark your classes with the `[GenerateArgumentsProvider]` attribute to generate optimized `IArgumentsProvider` implementations.

#### Quick Start

```csharp
using OrchardCore.DisplayManagement;

[GenerateArgumentsProvider]
public partial class MyShapeArguments
{
    public string Title { get; set; }
    public int Count { get; set; }
    public bool IsActive { get; set; }
}

// Usage - zero reflection overhead
var args = new MyShapeArguments 
{ 
    Title = "Hello", 
    Count = 42, 
    IsActive = true 
};

var shape = await shapeFactory.CreateAsync("MyShape", args);
```

**Requirements:**
- The class must be marked as `partial`
- At least one public instance property with a getter

### Alternative: Anonymous Types with Interceptors

For .NET 9+ projects, anonymous types are automatically optimized using C# interceptors:

```csharp
// Automatically optimized at compile time (no code changes needed)
var shape = await factory.CreateAsync("MyShape", new 
{ 
    Title = "Hello",
    Count = 42 
});
```

**Requirements:**
- .NET 9.0 or later
- C# 13 or later

**How it works:** The interceptor generator detects `Arguments.From(anonymousType)` calls and generates optimized code using type inference to cast to the actual anonymous type, avoiding both reflection and dynamic overhead.

### Fallback: Reflection with Caching

If you don't use source generation or interceptors, OrchardCore automatically uses cached reflection:

```csharp
// Works on any .NET version - uses reflection + caching
var args = Arguments.From(new { Title = "Test", Count = 5 });
```

This is suitable for prototyping or infrequent usage where performance isn't critical.

## Performance Comparison

| Approach | Performance | Setup | Stability | Use Case |
|----------|-------------|-------|-----------|----------|
| **Named Types with `[GenerateArgumentsProvider]`** | ⚡⚡⚡ Fastest | Add attribute | 🟢 Stable | **Production (Recommended)** |
| **Interceptors** | ⚡⚡⚡ Fastest | None (.NET 9+) | 🟢 Stable | .NET 9+ anonymous types |
| **Reflection + Cache** | ⚡ Cached | None | 🟢 Stable | Prototyping, rare usage |

## Real-World Examples

OrchardCore modules use source-generated models extensively. For example:

### Pager Shape

```csharp
[GenerateArgumentsProvider]
public partial class PagerSlim
{
    public int PageSize { get; set; }
    public string Before { get; set; }
    public string After { get; set; }
}
```

## What Gets Generated

When you mark a class with `[GenerateArgumentsProvider]`, the generator basically creates:

```csharp
// Your code:
[GenerateArgumentsProvider]
public partial class MyData
{
    public string Name { get; set; }
    public int Value { get; set; }
}

// Generated automatically:
public partial class MyData : IArgumentsProvider
{
    INamedEnumerable<object> IArgumentsProvider.GetArguments()
    {
        return Arguments.From(
            [Name, Value],
            ["Name", "Value"]
        );
    }
}
```

## Migration Guide

### From Anonymous Types to Named Types

```csharp
// Before:
var shape = await factory.CreateAsync("MyShape", new
{
    Title = "Hello",
    Count = 5
});

// After (Recommended):
[GenerateArgumentsProvider]
public partial class MyShapeArguments
{
    public string Title { get; set; }
    public int Count { get; set; }
}

var args = new MyShapeArguments { Title = "Hello", Count = 5 };
var shape = await factory.CreateAsync("MyShape", args);
```

## Interceptors for Anonymous Types (.NET 9+)

### How It Works

The interceptor uses type inference to cast anonymous objects without knowing their compiler-generated type names:

```csharp
// Helper that uses type inference
public static T Cast<T>(object obj, T type) => (T)obj;

// Generated interceptor
var typedObject = Cast(anonymousObject, new { Name = (string)default, Value = (int)default });

// Now we can access properties with zero reflection!
return Arguments.From(
    [typedObject.Name, typedObject.Value],
    ["Name", "Value"]
);
```

This clever trick:
1. Creates a "shape" instance with default values
2. Compiler infers the anonymous type from the shape
3. Casts the parameter to that type
4. Accesses properties with full type safety

**One small allocation** (the default instance) is far cheaper than reflection or dynamic overhead.

### Requirements

- .NET 9.0 or later
- C# 13 or later (no preview features needed)

### Verifying Interceptors

Check for generated files after building:

```
obj/Debug/net10.0/generated/
  OrchardCore.SourceGenerators/
    OrchardCore.DisplayManagement.SourceGenerators.ArgumentsFromInterceptor/
      ArgumentsFromInterceptors.g.cs
```

## Best Practices

1. **Use `[GenerateArgumentsProvider]` for production code** - Most stable and performant
2. **Use descriptive names** - `UserRegistrationEmailData` over `Data`
3. **Group related properties** - Create focused models for specific scenarios
4. **Keep models simple** - Avoid complex logic in getters
5. **Document usage** - Add XML comments explaining the model's purpose
6. **Look at OrchardCore examples** - Check existing modules for reference patterns

## Troubleshooting

### "Type must be partial"

**Solution:** Add the `partial` keyword:

```csharp
// ✅ Correct
[GenerateArgumentsProvider]
public partial class MyData { ... }

// ❌ Wrong
[GenerateArgumentsProvider]
public class MyData { ... }
```

### Generated code not found

1. Rebuild your project
2. Close and reopen files
3. Restart Visual Studio if needed
4. Verify the source generator project reference

### Interceptors not working (.NET 9+)

1. Verify you're using .NET 9.0+ and C# 13+
2. Clean and rebuild the solution
3. Check for source generator errors in build output

## When to Use Each Approach

```
Production code with reusable types?
├─ Yes → Use [GenerateArgumentsProvider] ✅ RECOMMENDED
└─ No
   └─ Using .NET 9+ with anonymous types?
      ├─ Yes → Interceptors (automatic) ✅
      └─ No → Reflection with caching (acceptable for rare usage)
```

## Available Generators

### ArgumentsProviderGenerator

Generates `IArgumentsProvider` implementations for types marked with `[GenerateArgumentsProvider]`.

**Status:** ✅ Stable, production-ready

### ArgumentsFromInterceptor

Intercepts `Arguments.From(anonymousType)` calls and optimizes them using type inference.

**Status:** ✅ Stable (.NET 9+)
