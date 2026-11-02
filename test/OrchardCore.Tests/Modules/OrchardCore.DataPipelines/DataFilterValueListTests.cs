using OrchardCore.DataPipelines.Steps;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class DataFilterValueListTests
{
    [Theory]
    [InlineData("Paris, London ,Rome", new[] { "Paris", "London", "Rome" })]
    [InlineData("Paris, \"Smith, John\"", new[] { "Paris", "Smith, John" })]
    [InlineData("  \"Say \"\"hi\"\"\" , 2", new[] { "Say \"hi\"", "2" })]
    [InlineData("\" padded \"", new[] { " padded " })]
    [InlineData("a,,b, ", new[] { "a", "b" })]
    [InlineData("{Parameter:Regions}", new[] { "{Parameter:Regions}" })]
    [InlineData("", new string[0])]
    [InlineData(null, new string[0])]
    public void Parse_Text_ReturnsTheValues(string text, string[] expected)
    {
        // Act
        var values = DataFilterValueList.Parse(text);

        // Assert
        Assert.Equal(expected, values);
    }

    [Fact]
    public void Format_Values_WritesATextThatParsesBack()
    {
        // Arrange
        string[] values = ["Paris", "Smith, John", "Say \"hi\"", " padded "];

        // Act
        var text = DataFilterValueList.Format(values);

        // Assert
        Assert.Equal("Paris, \"Smith, John\", \"Say \"\"hi\"\"\", \" padded \"", text);
        Assert.Equal(values, DataFilterValueList.Parse(text));
    }
}
