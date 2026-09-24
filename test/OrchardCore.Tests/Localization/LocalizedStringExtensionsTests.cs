namespace OrchardCore.Tests.Localization;

public class LocalizedStringExtensionsTests
{
    [Theory]
    [InlineData("Hello")]
    [InlineData("Hello {0}")]
    [InlineData("")]
    public void Create_WithValue_UsesValueAsName(string value)
    {
        // Act
        var localizedString = LocalizedString.Create(value);

        // Assert
        Assert.Equal(value, localizedString.Name);
        Assert.Equal(value, localizedString.Value);
        Assert.False(localizedString.ResourceNotFound);
        Assert.Null(localizedString.SearchedLocation);
    }

    [Fact]
    public void Create_WithArguments_FormatsValueAndKeepsName()
    {
        // Act
        var localizedString = LocalizedString.Create("Hello {0}, you have {1} messages", "Mike", 3);

        // Assert
        Assert.Equal("Hello {0}, you have {1} messages", localizedString.Name);
        Assert.Equal("Hello Mike, you have 3 messages", localizedString.Value);
    }

    [Fact]
    public void Create_WithNullValue_Throws()
    {
        Assert.Throws<ArgumentNullException>("value", () => LocalizedString.Create(null));
    }

    [Fact]
    public void Create_CalledOnExtensionClass_UsesValueAsName()
    {
        // Act
        var localizedString = LocalizedStringExtensions.Create("Hello {0}", "World");

        // Assert
        Assert.Equal("Hello {0}", localizedString.Name);
        Assert.Equal("Hello World", localizedString.Value);
    }
}
