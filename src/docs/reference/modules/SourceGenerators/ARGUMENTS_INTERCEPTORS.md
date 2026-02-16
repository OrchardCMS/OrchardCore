# Arguments.From Interceptors

## Overview

The `ArgumentsFromInterceptor` is a source generator that uses **C# interceptors** to optimize calls to `Arguments.From<T>()` when used with anonymous types. This eliminates the reflection overhead completely for anonymous types by generating named types and intercepting the calls at compile time.

## Requirements

- .NET 9.0 or later
- C# 13 or later

Interceptors are a stable feature in .NET 9+ and C# 13+. No special preview features are required.

## How It Works

### Without Interceptors (Fallback Behavior)

```csharp
// This uses reflection with caching
var args = Arguments.From(new { Name = "Test", Value = 42 });
```

At runtime, this:
1. Detects the anonymous type
2. Uses cached reflection to read properties
3. Creates the `INamedEnumerable<object>`

### With Interceptors (Optimized)

The source generator:

1. **Detects** all `Arguments.From(anonymousType)` calls during compilation
2. **Generates** a named type with the same properties:

```csharp
[GenerateArgumentsProvider]
file sealed class AnonymousArgumentsType0
{
    public AnonymousArgumentsType0(string name, int value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }
    public int Value { get; }
}
```

3. **Intercepts** the original call with compile-time generated code:

```csharp
[InterceptsLocation("MyFile.cs", 42, 15)]
public static INamedEnumerable<object> InterceptFrom<T>(T anonymousObject)
{
    var typed = new AnonymousArgumentsType0(
        ((dynamic)anonymousObject).Name,
        ((dynamic)anonymousObject).Value
    );
    
    return ((IArgumentsProvider)typed).GetArguments();
}
```

## Performance Benefits

- **Zero reflection**: Properties are accessed at compile time
- **Type safety**: Generated code is strongly typed
- **Optimal memory**: No caching overhead needed
- **Same API**: No code changes required

## Usage Examples

### Basic Usage

```csharp
// Your code stays the same - automatically optimized
var shape = await _shapeFactory.CreateAsync("MyShape", new 
{ 
    Title = "Hello",
    Count = 10,
    IsActive = true 
});
```

The interceptor automatically optimizes this call.

### Multiple Properties

```csharp
var args = Arguments.From(new
{
    Id = 123,
    Name = "Product",
    Price = 99.99m,
    Tags = new[] { "featured", "sale" },
    Metadata = new { Created = DateTime.Now }
});
```

Each unique anonymous type signature gets its own generated type.

### Reusable Signatures

If you use the same anonymous type shape multiple times:

```csharp
// First usage
var args1 = Arguments.From(new { X = 1, Y = 2 });

// Second usage - same signature, reuses generated type
var args2 = Arguments.From(new { X = 10, Y = 20 });
```

Both calls will be intercepted to use the same generated `AnonymousArgumentsType`.

## Comparison with Named Types

### Using Interceptors (Automatic)

```csharp
// Simple, clean code
var args = Arguments.From(new { Name = "Test", Value = 42 });
```

**Pros:**
- No boilerplate
- Works with existing code
- Zero reflection overhead

**Cons:**
- Dynamic property access in generated code
- Generated types are `file`-scoped

### Using Named Types with `[GenerateArgumentsProvider]`

```csharp
[GenerateArgumentsProvider]
public class MyArguments
{
    public string Name { get; set; }
    public int Value { get; set; }
}

var args = Arguments.From(new MyArguments { Name = "Test", Value = 42 });
```

**Pros:**
- Fully strongly typed
- Reusable across files
- No dynamic code

**Cons:**
- Requires defining types
- More code to write

## Limitations

1. **File-Scoped Types**: Generated types use `file` scope, so they're only visible within the generated file

2. **Dynamic Access**: The interceptor uses `dynamic` to access properties from the anonymous type, which has a small runtime cost (but still faster than reflection)

3. **Single Compilation**: Interceptors are tied to specific file locations during compilation

4. **No Nested Anonymous Types**: Complex nested structures might not work optimally

## When to Use

### ✅ Use Interceptors When:

- You have lots of anonymous type usage with `Arguments.From`
- You want optimal performance without code changes
- You're using .NET 9+ and C# 13+
- Your anonymous types are relatively simple

### ✅ Use Named Types with `[GenerateArgumentsProvider]` When:

- You want full type safety without `dynamic`
- You're defining reusable argument types
- You need types that are reusable across files

### ✅ Use Reflection Fallback When:

- You have complex or rare anonymous type usage
- The performance overhead is acceptable
- You want the simplest code without attributes

## Troubleshooting

### Interceptors Not Working

1. Verify you're using .NET 9.0 or later and C# 13 or later
2. Clean and rebuild the solution
3. Check for source generator errors in the build output

### Build Errors

If you see errors about interceptors, ensure your project is targeting the correct framework:

```xml
<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>13</LangVersion>
</PropertyGroup>
```

### Performance Not Improved

If you're not seeing performance improvements:

1. Verify the interceptor is actually being generated (check the generated source)
2. Make sure you're using anonymous types, not named types
3. Check that the call site matches exactly (file path and location)

## Migration Guide

### No Changes Needed!

Since interceptors are automatically applied when the generator detects anonymous types, you don't need to make any code changes. Simply:

1. Ensure you're on .NET 9+ with C# 13+
2. Rebuild your project
3. Your `Arguments.From(new { ... })` calls are now optimized!

### From Interceptors to Named Types

If you want to move to named types:

**Before (with interceptor):**
```csharp
var args = Arguments.From(new { Name = "Test", Value = 42 });
```

**After (with named type):**
```csharp
[GenerateArgumentsProvider]
public class TestArguments
{
    public string Name { get; set; }
    public int Value { get; set; }
}

var args = Arguments.From(new TestArguments { Name = "Test", Value = 42 });
```

## See Also

- [Arguments Optimization Guide](ARGUMENTS_OPTIMIZATION.md)
- [Arguments Source Generation](ARGUMENTS_SOURCE_GENERATION.md)
- [C# Interceptors Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#interceptors)
