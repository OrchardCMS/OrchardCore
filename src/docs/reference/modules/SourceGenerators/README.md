# OrchardCore Source Generators

OrchardCore includes source generators that optimize performance by generating code at compile time, eliminating reflection overhead in critical paths.

## Arguments Source Generation

### Overview

The `Arguments` class is used throughout OrchardCore to convert objects into named argument collections for shapes and other scenarios. Source generation provides compile-time generated property access to avoid reflection overhead.

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

// Usage - direct property access, no reflection
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

**Benefits:**
- ✅ **No reflection** - Direct property access via switch expressions
- ✅ **Compile-time code generation** - Errors caught at build time
- ✅ **Lazy evaluation** - Properties accessed on-demand
- ✅ **Type safety** - Strongly typed arguments with IntelliSense

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
- Property names stored as static array
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
- ✅ **No reflection** - Direct property access

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
- Uses linear search for property lookups (simple and efficient for typical 2-15 properties)
- Handles enumeration and collection operations
- Only requires derived classes to implement 3 simple methods
- Minimal allocation design - no intermediate dictionaries or arrays until accessed

### Linear Search for Property Access

The generated code uses a linear search (sequential scan) when looking up properties by name:

```csharp
public bool TryGetValue(string key, out object value)
{
    var names = _parent.PropertyNames;
    for (var i = 0; i < names.Count; i++)
    {
        if (string.Equals(names[i], key, StringComparison.Ordinal))
        {
            value = _parent.GetPropertyValue(i);
            return true;
        }
    }
    value = default;
    return false;
}
```

**Why linear search?**

For typical OrchardCore shape arguments (2-15 properties):
- **Simplicity** - Easy to understand and maintain
- **No allocation overhead** - No dictionary initialization unless needed
- **Good cache locality** - Sequential memory access
- **Adequate performance** - For small property counts, sequential scan is fast

## Best Practices

1. **Use `[GenerateArguments]` for production code** - Recommended for reusable shape arguments
2. **Use descriptive names** - `UserRegistrationEmailData` over `Data`
3. **Group related properties** - Create focused models for specific scenarios
4. **Keep models simple** - Avoid complex logic in getters
5. **Document usage** - Add XML comments explaining the model's purpose
6. **Look at OrchardCore examples** - Check existing modules for reference patterns
7. **Consider reusability** - Named types can be shared across multiple call sites
8. **Keep property counts reasonable** - Designed for typical shape arguments with 2-20 properties

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

**Performance:** Zero allocation, O(n) linear search optimized for small property counts

### ArgumentsFromInterceptor

Intercepts `Arguments.From(anonymousType)` calls and optimizes them using type inference.

**Status:** ✅ Stable (.NET 9+)

**Generated Code Size:** ~20 lines per call site

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
