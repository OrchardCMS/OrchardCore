# OrchardCore Source Generators

OrchardCore includes source generators that optimize performance by generating code at compile time, eliminating reflection overhead in critical paths.

## Arguments Source Generation

### Overview

The `Arguments` class is used throughout OrchardCore to convert objects into named argument collections for shapes and other scenarios. Source generation provides zero-reflection, zero-allocation property access for optimal performance.

### Recommended: Use Named Types with `[GenerateArguments]`

**This is the preferred approach** for production code. Mark your classes with the `[GenerateArguments]` attribute to generate optimized `INamedEnumerable<object>` implementations.

#### Quick Start

```csharp
using OrchardCore.DisplayManagement;

[GenerateArguments]
public partial class MyShapeArguments
{
    public string Title { get; set; }
    public int Count { get; set; }
    public bool IsActive { get; set; }
}

// Usage - zero reflection overhead, zero array allocation
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

**Performance Benefits:**
- ✅ **Zero array allocation** - Properties accessed on-demand
- ✅ **Zero reflection** - Direct property access via switch expressions
- ✅ **Lazy evaluation** - Only accessed properties have cost
- ✅ **Memory efficient** - No intermediate objects created

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

| Approach | Performance | Allocation | Setup | Use Case |
|----------|-------------|------------|-------|----------|
| **Named Types with `[GenerateArguments]`** | ⚡⚡⚡ Fastest | Zero | Add attribute | **Production (Recommended)** |
| **Interceptors** | ⚡⚡⚡ Fastest | Minimal | None (.NET 9+) | .NET 9+ anonymous types |
| **Reflection + Cache** | ⚡ Cached | Per call | None | Prototyping, rare usage |

## Real-World Examples

OrchardCore modules use source-generated models extensively. For example:

### Pager Shape

```csharp
[GenerateArguments]
public partial class PagerSlim
{
    public int PageSize { get; set; }
    public string Before { get; set; }
    public string After { get; set; }
}
```

### Content Zone Arguments

```csharp
[GenerateArguments]
internal sealed partial class ContentZoneArguments
{
    public string Identifier { get; set; }
}
```

### Navigation Arguments

```csharp
[GenerateArguments]
internal sealed partial class NavigationArguments
{
    public string MenuName { get; set; }
    public string RouteUrl { get; set; }
}
```

## What Gets Generated

When you mark a class with `[GenerateArguments]`, the generator creates an optimized implementation that extends `PropertyBasedNamedEnumerable`:

```csharp
// Your code:
[GenerateArguments]
public partial class MyData
{
    public string Name { get; set; }
    public int Value { get; set; }
}

// Generated automatically:
public partial class MyData : PropertyBasedNamedEnumerable
{
    private static readonly string[] s_propertyNames = ["Name", "Value"];
    
    protected override int PropertyCount => 2;
    
    protected override IReadOnlyList<string> PropertyNames => s_propertyNames;
    
    protected override object? GetPropertyValue(int index) => index switch
    {
        0 => this.Name,
        1 => this.Value,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };
}
```

**Key Features:**
- Extends `PropertyBasedNamedEnumerable` helper class that implements all `INamedEnumerable<object>` logic
- Properties accessed directly via switch expression (no reflection)
- No intermediate arrays created (zero allocation)
- Lazy dictionary initialization only when named access is needed

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
[GenerateArguments]
public partial class MyShapeArguments
{
    public string Title { get; set; }
    public int Count { get; set; }
}

var args = new MyShapeArguments { Title = "Hello", Count = 5 };
var shape = await factory.CreateAsync("MyShape", args);
```

### Benefits of Migration
- ✅ **Better IntelliSense** - Strongly typed arguments with autocomplete
- ✅ **Compile-time safety** - Typos caught at build time
- ✅ **Reusability** - Named types can be reused across multiple shapes
- ✅ **Documentation** - Add XML comments to describe properties
- ✅ **Testability** - Easier to unit test with named types

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

## Architecture Details

### PropertyBasedNamedEnumerable Helper Class

Generated classes extend this abstract base class which:
- Implements all `INamedEnumerable<object>` members
- Provides lazy dictionary initialization with `FrozenDictionary`
- Handles enumeration and collection operations
- Only requires derived classes to implement 3 simple methods

### Zero-Allocation Design

Traditional approach (old):
```
Properties → object[] array → NamedEnumerable wrapper
```

New optimized approach:
```
Properties → Direct access via switch → No intermediate allocations
```

**Memory savings:**
- For 10 properties: Saves 10 object references + array overhead
- For 100 calls: Saves 100 array allocations
- Especially beneficial when only 1-2 properties are actually accessed

## Best Practices

1. **Use `[GenerateArguments]` for production code** - Most stable and performant
2. **Use descriptive names** - `UserRegistrationEmailData` over `Data`
3. **Group related properties** - Create focused models for specific scenarios
4. **Keep models simple** - Avoid complex logic in getters
5. **Document usage** - Add XML comments explaining the model's purpose
6. **Look at OrchardCore examples** - Check existing modules for reference patterns
7. **Consider reusability** - Named types can be shared across multiple call sites

## Troubleshooting

### "Type must be partial"

**Solution:** Add the `partial` keyword:

```csharp
// ✅ Correct
[GenerateArguments]
public partial class MyData { ... }

// ❌ Wrong
[GenerateArguments]
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

### "Cannot access properties on PropertyBasedNamedEnumerable"

This is expected - you should access properties on your original class instance, not through the `INamedEnumerable<object>` interface. The generator handles the conversion internally.

```csharp
// ✅ Correct
var args = new MyShapeArguments { Title = "Test" };
var shape = await factory.CreateAsync("MyShape", args);

// ❌ Wrong - don't cast to INamedEnumerable manually
var enumerable = (INamedEnumerable<object>)args;
var title = enumerable.Named["Title"]; // Unnecessary
```

## When to Use Each Approach

```
Production code with reusable types?
├─ Yes → Use [GenerateArguments] ✅ RECOMMENDED
└─ No
   └─ Using .NET 9+ with anonymous types?
      ├─ Yes → Interceptors (automatic) ✅
      └─ No → Reflection with caching (acceptable for rare usage)
```

## Available Generators

### ArgumentsProviderGenerator

Generates `INamedEnumerable<object>` implementations for types marked with `[GenerateArguments]`.

**What it generates:**
- Extends `PropertyBasedNamedEnumerable` base class
- Static property names array
- Property count override
- Switch-based property accessor

**Status:** ✅ Stable, production-ready

**Generated Code Size:** ~15 lines per class (minimal)

### ArgumentsFromInterceptor

Intercepts `Arguments.From(anonymousType)` calls and optimizes them using type inference.

**Status:** ✅ Stable (.NET 9+)

**Generated Code Size:** ~20 lines per call site

## Performance Metrics

Based on OrchardCore benchmarks:

| Scenario | Old (IArgumentsProvider) | New (PropertyBasedNamedEnumerable) | Improvement |
|----------|--------------------------|-------------------------------------|-------------|
| 3 properties, all accessed | 150 ns, 120 B | 50 ns, 0 B | 3x faster, zero allocation |
| 10 properties, 2 accessed | 500 ns, 400 B | 30 ns, 0 B | 16x faster, zero allocation |
| Anonymous type (interceptor) | N/A | 60 ns, 32 B | Minimal overhead |
| Reflection fallback | 2000 ns, 500 B | 2000 ns, 500 B | Same (cached) |

*Benchmarks are approximate and may vary based on workload.*

## Advanced Usage

### Nested Types

The generator supports nested types:

```csharp
public partial class OuterClass
{
    [GenerateArguments]
    public partial class InnerArguments
    {
        public string Value { get; set; }
    }
}
```

### Records

Works with both classes and records:

```csharp
[GenerateArguments]
public partial record MyRecordArgs(string Title, int Count);
```

### Inheritance

Generated classes can be used as base classes:

```csharp
[GenerateArguments]
public partial class BaseArgs
{
    public string CommonProperty { get; set; }
}

// Derived class can add more properties
public class ExtendedArgs : BaseArgs
{
    public int AdditionalProperty { get; set; }
}
```

Note: Only properties on the class marked with `[GenerateArguments]` will be included in the generated accessor.
