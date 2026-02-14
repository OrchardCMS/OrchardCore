using OrchardCore.DisplayManagement;

namespace OrchardCore.Tests.DisplayManagement;

public partial class ArgumentsTests
{
    [Fact]
    public void From_WithAnonymousObject_ReturnsNamedEnumerable()
    {
        // Arrange
        var obj = new { Name = "Test", Value = 42 };

        // Act
        var result = global::OrchardCore.DisplayManagement.Arguments.From(obj);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Named.Count);
        Assert.Equal("Test", result.Named["Name"]);
        Assert.Equal(42, result.Named["Value"]);
    }

    [Fact]
    public void From_WithArgumentsProvider_UsesOptimizedPath()
    {
        // Arrange
        var obj = new TestArgumentsProvider { Name = "Test", Value = 42 };

        // Act
        var result = global::OrchardCore.DisplayManagement.Arguments.From(obj);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Named.Count);
        Assert.Equal("Test", result.Named["Name"]);
        Assert.Equal(42, result.Named["Value"]);
        Assert.True(obj.GetArgumentsCalled);
    }

    [Fact]
    public void From_WithDictionary_ReturnsNamedEnumerable()
    {
        // Arrange
        var dict = new Dictionary<string, object>
        {
            ["Name"] = "Test",
            ["Value"] = 42,
        };

        // Act
        var result = global::OrchardCore.DisplayManagement.Arguments.From(dict);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Named.Count);
        Assert.Equal("Test", result.Named["Name"]);
        Assert.Equal(42, result.Named["Value"]);
    }

    [Fact]
    public void From_WithArraysAndNames_ReturnsNamedEnumerable()
    {
        // Arrange
        var values = new object[] { "Test", 42 };
        var names = new[] { "Name", "Value" };

        // Act
        var result = global::OrchardCore.DisplayManagement.Arguments.From(values, names);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Named.Count);
        Assert.Equal("Test", result.Named["Name"]);
        Assert.Equal(42, result.Named["Value"]);
    }

    [Fact]
    public void ToArguments_Extension_WithObject_ReturnsNamedEnumerable()
    {
        // Arrange
        var obj = new { Name = "Test", Value = 42 };

        // Act
        var result = obj.ToArguments();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Named.Count);
        Assert.Equal("Test", result.Named["Name"]);
        Assert.Equal(42, result.Named["Value"]);
    }

    [Fact]
    public void ArgumentsProviderHelper_Create_WithArrays_ReturnsNamedEnumerable()
    {
        // Arrange
        var values = new object[] { "Test", 42 };
        var names = new[] { "Name", "Value" };

        // Act
        var result = ArgumentsProviderHelper.Create(values, names);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Named.Count);
        Assert.Equal("Test", result.Named["Name"]);
        Assert.Equal(42, result.Named["Value"]);
    }

    [Fact]
    public void SourceGenerated_SimpleModel_ShouldUseGeneratedProvider()
    {
        // Arrange
        var model = new TestSourceGeneratedModel
        {
            Name = "Test",
            Value = 42,
            IsActive = true,
        };

        // Act
        var result = global::OrchardCore.DisplayManagement.Arguments.From(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Named.Count);
        Assert.Equal("Test", result.Named["Name"]);
        Assert.Equal(42, result.Named["Value"]);
        Assert.Equal(true, result.Named["IsActive"]);
        
        // Verify it's using IArgumentsProvider (not reflection)
        Assert.IsAssignableFrom<IArgumentsProvider>(model);
    }

    [Fact]
    public void SourceGenerated_ComplexModel_ShouldIncludeAllProperties()
    {
        // Arrange
        var model = new TestComplexSourceGeneratedModel
        {
            Id = "test-id",
            Title = "Test Title",
            Count = 100,
            CreatedDate = new DateTime(2024, 1, 1),
            Tags = ["tag1", "tag2"],
            Metadata = new Dictionary<string, string> { ["key"] = "value" },
        };

        // Act
        var result = global::OrchardCore.DisplayManagement.Arguments.From(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.Named.Count);
        Assert.Equal("test-id", result.Named["Id"]);
        Assert.Equal("Test Title", result.Named["Title"]);
        Assert.Equal(100, result.Named["Count"]);
        Assert.Equal(new DateTime(2024, 1, 1), result.Named["CreatedDate"]);
        Assert.Equal(new[] { "tag1", "tag2" }, result.Named["Tags"]);
    }

    [GenerateArgumentsProvider]
    private sealed partial class TestSourceGeneratedModel
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public bool IsActive { get; set; }
    }

    [GenerateArgumentsProvider]
    private sealed partial class TestComplexSourceGeneratedModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Count { get; set; }
        public DateTime CreatedDate { get; set; }
        public string[] Tags { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    private sealed class TestArgumentsProvider : IArgumentsProvider
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public bool GetArgumentsCalled { get; private set; }

        public INamedEnumerable<object> GetArguments()
        {
            GetArgumentsCalled = true;
            return ArgumentsProviderHelper.Create(
                [Name, (object)Value],
                [nameof(Name), nameof(Value)]
            );
        }
    }
}
