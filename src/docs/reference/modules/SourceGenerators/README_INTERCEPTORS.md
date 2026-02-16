# Enabling Arguments Interceptors

This document explains how to use the `ArgumentsFromInterceptor` feature for your OrchardCore modules or applications.

## Requirements

**C# Interceptors are a stable feature** in .NET 9+ and C# 13+. No preview features are required.

- .NET 9.0 or later
- C# 13 or later

## How It Works

The `ArgumentsFromInterceptor` source generator automatically detects `Arguments.From(new { ... })` calls and generates optimized code at compile time. No configuration is needed - it works automatically when you use anonymous types with `Arguments.From()`.

## Verifying It Works

### 1. Build the Project

```bash
dotnet build
```

Look for generated files in `obj/Debug/net10.0/generated/`:
```
OrchardCore.SourceGenerators/
  OrchardCore.DisplayManagement.SourceGenerators.ArgumentsFromInterceptor/
    ArgumentsFromInterceptors.g.cs
```

### 2. Check Generated Source

In Visual Studio:
1. Expand your project in Solution Explorer
2. Expand **Dependencies** → **Analyzers** → **OrchardCore.SourceGenerators**
3. Look for `ArgumentsFromInterceptors.g.cs`

### 3. Add Diagnostic Code

Add this to your code to verify interception:

```csharp
public void TestInterceptor()
{
    // Add a unique comment to make it easier to find in generated source
    var args = Arguments.From(new { Test = "Interceptor" }); // MARKER-001
    
    // Check if this was intercepted by looking at the generated source
}
```

Then search the generated `ArgumentsFromInterceptors.g.cs` for your file path.

## Performance Testing

Create a benchmark to verify the performance improvement:

```csharp
using BenchmarkDotNet.Attributes;
using OrchardCore.DisplayManagement;

[MemoryDiagnoser]
public class ArgumentsBenchmark
{
    [Benchmark(Baseline = true)]
    public void WithAnonymousType()
    {
        var result = Arguments.From(new { Name = "Test", Value = 42 });
    }

    [GenerateArgumentsProvider]
    public class NamedType
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    [Benchmark]
    public void WithNamedType()
    {
        var result = Arguments.From(new NamedType { Name = "Test", Value = 42 });
    }
}
```

Expected results:
- **With interceptors**: Anonymous type should be as fast as named type
- **Without interceptors**: Anonymous type will be slower (reflection + caching)

## Troubleshooting

### Interceptors Not Being Generated

1. **Clean the solution**:
   ```bash
   dotnet clean
   dotnet build
   ```

2. **Check source generator errors**:
   - In Visual Studio: View → Error List → Warnings
   - Look for source generator warnings

3. **Verify you're calling `Arguments.From` with anonymous types**:
   ```csharp
   // ✅ Will be intercepted
   Arguments.From(new { Name = "Test" })
   
   // ❌ Won't be intercepted (named type)
   Arguments.From(myNamedObject)
   
   // ❌ Won't be intercepted (implements IArgumentsProvider)
   Arguments.From(myArgumentsProvider)
   ```

4. **Check file paths**:
   - Interceptors are tied to specific file locations
   - Moving files or changing line numbers requires a rebuild
   - Rebuild after moving files

### Performance Not Improving

If you don't see performance improvements:

1. **Verify interceptors are being generated** (check generated source)
2. **Use a profiler** to confirm the interceptor is being called
3. **Compare with named type** (should be similar performance)
4. Remember: The dynamic property access in interceptors has some overhead

### Compiler Errors

If you see errors about interceptors:

```
error CS9234: Cannot use 'interceptors' in this compilation
```

**Solution**: Ensure you're using .NET 9.0+ and C# 13+:

```xml
<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>13</LangVersion>
</PropertyGroup>
```

## Migration Path

### No Migration Needed!

Interceptors work automatically with your existing code:

```csharp
// This code works without changes
var shape = await factory.CreateAsync("MyShape", new
{
    Title = "Hello",
    Count = 5
});
```

The interceptor will automatically optimize this at compile time.

### Optional: Convert to Named Types

If you want more control, you can convert to named types:

```csharp
// Before (with automatic interceptor)
var shape = await factory.CreateAsync("MyShape", new
{
    Title = "Hello",
    Count = 5
});

// After (with named type)
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

## See Also

- [Arguments Interceptors Documentation](../../../docs/reference/modules/DisplayManagement/ARGUMENTS_INTERCEPTORS.md)
- [Arguments Optimization Guide](../../../docs/reference/modules/DisplayManagement/ARGUMENTS_OPTIMIZATION.md)
- [C# Interceptors Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#interceptors)
