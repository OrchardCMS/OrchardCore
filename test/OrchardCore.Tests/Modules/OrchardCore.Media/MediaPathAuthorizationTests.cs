using OrchardCore.Media.Endpoints.Api;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaPathAuthorizationTests
{
    [Theory]
    [InlineData("image.png", true)]
    [InlineData("my image.png", true)]
    [InlineData("../image.png", false)]
    [InlineData("folder/image.png", false)]
    [InlineData(@"folder\image.png", false)]
    [InlineData(".", false)]
    [InlineData("..", false)]
    [InlineData("", false)]
    public void IsBaseName_Path_ReturnsExpectedResult(string name, bool expected)
    {
        Assert.Equal(expected, MediaEndpointHelpers.IsBaseName(name));
    }
}
