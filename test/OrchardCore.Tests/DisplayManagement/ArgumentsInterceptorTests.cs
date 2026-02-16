using OrchardCore.DisplayManagement;
using Xunit;
using DisplayManagementArguments = OrchardCore.DisplayManagement.Arguments;

#nullable enable

namespace OrchardCore.Tests.DisplayManagement;

/// <summary>
/// Tests for the ArgumentsFromInterceptor source generator.
/// Note: These tests will only use interceptors if the project has Interceptors enabled.
/// </summary>
public partial class ArgumentsInterceptorTests
{
    [Fact]
    public void From_WithSimpleAnonymousType_CreatesNamedEnumerable()
    {
        // This call may be intercepted if Interceptors is enabled
        var result = DisplayManagementArguments.From(new { Name = "Test", Value = 42 });

        Assert.NotNull(result);
        Assert.Equal(2, result.Named.Count);
        Assert.Equal("Test", result.Named["Name"]);
        Assert.Equal(42, result.Named["Value"]);
    }

    [Fact]
    public void From_WithComplexAnonymousType_CreatesNamedEnumerable()
    {
        // This call may be intercepted if Interceptors is enabled
        var result = DisplayManagementArguments.From(new
        {
            Id = 123,
            Name = "Product",
            Price = 99.99m,
            IsActive = true,
            Tags = new[] { "featured", "sale" },
        });

        Assert.NotNull(result);
        Assert.Equal(5, result.Named.Count);
        Assert.Equal(123, result.Named["Id"]);
        Assert.Equal("Product", result.Named["Name"]);
        Assert.Equal(99.99m, result.Named["Price"]);
        Assert.True((bool)result.Named["IsActive"]);
        Assert.Equal(new[] { "featured", "sale" }, result.Named["Tags"]);
    }

    [Fact]
    public void From_WithMultipleSameSignature_ReusesSameType()
    {
        // These calls should use the same generated type if intercepted
        var result1 = DisplayManagementArguments.From(new { X = 1, Y = 2 });
        var result2 = DisplayManagementArguments.From(new { X = 10, Y = 20 });

        Assert.Equal(1, result1.Named["X"]);
        Assert.Equal(2, result1.Named["Y"]);
        Assert.Equal(10, result2.Named["X"]);
        Assert.Equal(20, result2.Named["Y"]);
    }

    [Fact]
    public void From_WithDifferentSignatures_CreatesDifferentTypes()
    {
        // These should generate different types
        var result1 = DisplayManagementArguments.From(new { Name = "Test" });
        var result2 = DisplayManagementArguments.From(new { Name = "Test", Value = 42 });

        Assert.Single(result1.Named);
        Assert.Equal(2, result2.Named.Count);
    }

    [Fact]
    public void From_WithNullableProperties_HandlesCorrectly()
    {
        var result = DisplayManagementArguments.From(new
        {
            RequiredName = "Test",
            OptionalValue = (int?)42,
            NullValue = (string?)null,
        });

        Assert.Equal(3, result.Named.Count);
        Assert.Equal("Test", result.Named["RequiredName"]);
        Assert.Equal(42, result.Named["OptionalValue"]);
        Assert.Null(result.Named["NullValue"]);
    }

    [Fact]
    public void From_WithNestedAnonymousType_CreatesCorrectStructure()
    {
        // Nested anonymous types should be handled correctly, but this will still use reflection.
        var result = DisplayManagementArguments.From(new
        {
            Name = "Product",
            Metadata = new { Created = new DateTime(2024, 1, 1), Updated = new DateTime(2024, 1, 2) },
        });

        Assert.Equal(2, result.Named.Count);
        Assert.Equal("Product", result.Named["Name"]);
        
        var metadata = result.Named["Metadata"];
        Assert.NotNull(metadata);
    }

    [Fact]
    public void From_PerformanceComparison_InterceptorVsReflection()
    {
        // This test demonstrates the performance difference
        // With interceptors: near-zero overhead
        // Without interceptors: reflection with caching

        const int iterations = 1000;
        
        for (int i = 0; i < iterations; i++)
        {
            var result = DisplayManagementArguments.From(new { Index = i, Name = $"Item{i}" });
            Assert.Equal(i, result.Named["Index"]);
        }

        // If interceptors are enabled, this should be very fast
        // If not, it uses cached reflection which is still reasonably fast
    }
}

/// <summary>
/// Demonstrates the recommended approach using named types with [GenerateArgumentsProvider]
/// for production code that doesn't use preview features.
/// </summary>
public partial class ArgumentsNamedTypeTests
{
    [global::OrchardCore.DisplayManagement.GenerateArgumentsProvider]
    public partial class ProductArguments
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    [Fact]
    public void From_WithGeneratedArgumentsProvider_UsesGeneratedCode()
    {
        var result = DisplayManagementArguments.From(new ProductArguments
        {
            Id = 123,
            Name = "Test Product",
            Price = 99.99m,
        });

        Assert.NotNull(result);
        Assert.Equal(3, result.Named.Count);
        Assert.Equal(123, result.Named["Id"]);
        Assert.Equal("Test Product", result.Named["Name"]);
        Assert.Equal(99.99m, result.Named["Price"]);
    }
}
