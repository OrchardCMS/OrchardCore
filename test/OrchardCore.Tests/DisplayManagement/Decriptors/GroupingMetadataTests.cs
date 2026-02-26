using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Tests.DisplayManagement.Decriptors;

public class GroupingMetadataTests
{
    [Fact]
    public void Empty_ShouldReturnDefaultInstance()
    {
        var empty = GroupingMetadata.Empty;

        Assert.False(empty.HasValue);
        Assert.Null(empty.Name);
        Assert.Null(empty.Position);
        Assert.Null(empty.Width);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ParseTabOrCard_WithNullOrEmpty_ReturnsEmpty(string value)
    {
        var result = GroupingMetadata.ParseTabOrCard(value);

        Assert.False(result.HasValue);
        Assert.Equal(GroupingMetadata.Empty, result);
    }

    [Theory]
    [InlineData("Settings", "Settings", null)]
    [InlineData("Content", "Content", null)]
    [InlineData("Tab1", "Tab1", null)]
    public void ParseTabOrCard_WithNameOnly_ReturnsCorrectName(string input, string expectedName, string expectedPosition)
    {
        var result = GroupingMetadata.ParseTabOrCard(input);

        Assert.True(result.HasValue);
        Assert.Equal(expectedName, result.Name);
        Assert.Equal(expectedPosition, result.Position);
        Assert.Null(result.Width);
    }

    [Theory]
    [InlineData("Settings;1", "Settings", "1")]
    [InlineData("Content;5", "Content", "5")]
    [InlineData("Tab1;2.5", "Tab1", "2.5")]
    [InlineData("Details;before", "Details", "before")]
    [InlineData("Other;after", "Other", "after")]
    public void ParseTabOrCard_WithPosition_ReturnsCorrectValues(string input, string expectedName, string expectedPosition)
    {
        var result = GroupingMetadata.ParseTabOrCard(input);

        Assert.True(result.HasValue);
        Assert.Equal(expectedName, result.Name);
        Assert.Equal(expectedPosition, result.Position);
        Assert.Null(result.Width);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ParseColumn_WithNullOrEmpty_ReturnsEmpty(string value)
    {
        var result = GroupingMetadata.ParseColumn(value);

        Assert.False(result.HasValue);
        Assert.Equal(GroupingMetadata.Empty, result);
    }

    [Theory]
    [InlineData("Left", "Left", null, null)]
    [InlineData("Right", "Right", null, null)]
    [InlineData("Content", "Content", null, null)]
    public void ParseColumn_WithNameOnly_ReturnsCorrectName(string input, string expectedName, string expectedPosition, string expectedWidth)
    {
        var result = GroupingMetadata.ParseColumn(input);

        Assert.True(result.HasValue);
        Assert.Equal(expectedName, result.Name);
        Assert.Equal(expectedPosition, result.Position);
        Assert.Equal(expectedWidth, result.Width);
    }

    [Theory]
    [InlineData("Left;1", "Left", "1", null)]
    [InlineData("Content;5.1", "Content", "5.1", null)]
    [InlineData("Right;before", "Right", "before", null)]
    public void ParseColumn_WithPositionOnly_ReturnsCorrectValues(string input, string expectedName, string expectedPosition, string expectedWidth)
    {
        var result = GroupingMetadata.ParseColumn(input);

        Assert.True(result.HasValue);
        Assert.Equal(expectedName, result.Name);
        Assert.Equal(expectedPosition, result.Position);
        Assert.Equal(expectedWidth, result.Width);
    }

    [Theory]
    [InlineData("Left_9", "Left", null, "9")]
    [InlineData("Content_3", "Content", null, "3")]
    [InlineData("Right_lg-6", "Right", null, "lg-6")]
    public void ParseColumn_WithWidthOnly_ReturnsCorrectValues(string input, string expectedName, string expectedPosition, string expectedWidth)
    {
        var result = GroupingMetadata.ParseColumn(input);

        Assert.True(result.HasValue);
        Assert.Equal(expectedName, result.Name);
        Assert.Equal(expectedPosition, result.Position);
        Assert.Equal(expectedWidth, result.Width);
    }

    [Theory]
    [InlineData("Left_9;1", "Left", "1", "9")]
    [InlineData("Content_3;5", "Content", "5", "3")]
    [InlineData("Right_lg-6;2.5", "Right", "2.5", "lg-6")]
    public void ParseColumn_WithWidthThenPosition_ReturnsCorrectValues(string input, string expectedName, string expectedPosition, string expectedWidth)
    {
        var result = GroupingMetadata.ParseColumn(input);

        Assert.True(result.HasValue);
        Assert.Equal(expectedName, result.Name);
        Assert.Equal(expectedPosition, result.Position);
        Assert.Equal(expectedWidth, result.Width);
    }

    [Theory]
    [InlineData("Left;1_9", "Left", "1", "9")]
    [InlineData("Content;5_3", "Content", "5", "3")]
    [InlineData("Right;2.5_lg-6", "Right", "2.5", "lg-6")]
    public void ParseColumn_WithPositionThenWidth_ReturnsCorrectValues(string input, string expectedName, string expectedPosition, string expectedWidth)
    {
        var result = GroupingMetadata.ParseColumn(input);

        Assert.True(result.HasValue);
        Assert.Equal(expectedName, result.Name);
        Assert.Equal(expectedPosition, result.Position);
        Assert.Equal(expectedWidth, result.Width);
    }

    [Theory]
    [InlineData("Settings", "Settings")]
    [InlineData("Settings;1", "Settings;1")]
    public void ToString_TabOrCard_ReturnsOriginalFormat(string input, string expected)
    {
        var result = GroupingMetadata.ParseTabOrCard(input);
        Assert.Equal(expected, result.ToString());
    }

    [Theory]
    [InlineData("Left", "Left")]
    [InlineData("Left;1", "Left;1")]
    [InlineData("Left_9", "Left_9")]
    [InlineData("Left_9;1", "Left_9;1")]
    public void ToString_Column_ReturnsExpectedFormat(string input, string expected)
    {
        var result = GroupingMetadata.ParseColumn(input);
        Assert.Equal(expected, result.ToString());
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var a = new GroupingMetadata("Test", "1", "9");
        var b = new GroupingMetadata("Test", "1", "9");

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var a = new GroupingMetadata("Test", "1", "9");
        var b = new GroupingMetadata("Test", "2", "9");

        Assert.False(a.Equals(b));
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        var a = new GroupingMetadata("Test", "1", "9");
        var b = new GroupingMetadata("Test", "1", "9");

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Theory]
    [InlineData("Settings")]
    [InlineData("Settings;1")]
    public void ImplicitConversion_FromString_ParsesCorrectly(string value)
    {
        GroupingMetadata metadata = value;

        Assert.True(metadata.HasValue);
        Assert.Equal(GroupingMetadata.ParseTabOrCard(value).Name, metadata.Name);
        Assert.Equal(GroupingMetadata.ParseTabOrCard(value).Position, metadata.Position);
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsCorrectValue()
    {
        var metadata = new GroupingMetadata("Settings", "1");
        string value = metadata;

        Assert.Equal("Settings;1", value);
    }

    [Fact]
    public void ImplicitConversion_EmptyToString_ReturnsNull()
    {
        var metadata = GroupingMetadata.Empty;
        string value = metadata;

        Assert.Null(value);
    }

    [Fact]
    public void ImplicitConversion_FromNull_ReturnsEmpty()
    {
        GroupingMetadata metadata = (string)null;

        Assert.False(metadata.HasValue);
        Assert.Equal(GroupingMetadata.Empty, metadata);
    }

    [Theory]
    [InlineData("Settings", "Settings")]
    [InlineData("Settings;1", "Settings;1")]
    public void ToLocationString_ReturnsCorrectFormat(string input, string expected)
    {
        var result = GroupingMetadata.ParseTabOrCard(input);
        Assert.Equal(expected, result.ToString());
    }

    [Fact]
    public void ToLocationString_Empty_ReturnsNull()
    {
        var result = GroupingMetadata.Empty;
        Assert.Null(result.ToString());
    }

    [Theory]
    [InlineData("Left_9;1", "Left_9;1")]
    [InlineData("Content_3", "Content_3")]
    public void ToLocationString_Column_ReturnsCorrectFormat(string input, string expected)
    {
        var result = GroupingMetadata.ParseColumn(input);
        Assert.Equal(expected, result.ToString());
    }
}
