namespace OrchardCore.Tests.Localization;

public class LocalizedStringExtensionsTests
{
    [Theory]
    [InlineData("Hello")]
    [InlineData("Hello {0}")]
    [InlineData("")]
    public void Create_WithName_UsesNameAsValue(string name)
    {
        // Act
        var localizedString = LocalizedString.Create(name);

        // Assert
        Assert.Equal(name, localizedString.Name);
        Assert.Equal(name, localizedString.Value);
        Assert.False(localizedString.ResourceNotFound);
        Assert.Null(localizedString.SearchedLocation);
    }

    [Fact]
    public void Create_WithNullName_Throws()
    {
        Assert.Throws<ArgumentNullException>("name", () => LocalizedString.Create(null));
    }

    [Fact]
    public void Create_CalledOnExtensionClass_UsesNameAsValue()
    {
        // Act
        var localizedString = LocalizedStringExtensions.Create("Hello");

        // Assert
        Assert.Equal("Hello", localizedString.Name);
        Assert.Equal("Hello", localizedString.Value);
    }
}
