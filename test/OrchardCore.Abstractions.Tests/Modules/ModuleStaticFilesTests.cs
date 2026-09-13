using Xunit;

namespace OrchardCore.Modules;

public class ModuleStaticFilesTests
{
    [Theory]
    [InlineData("css/site.css")]
    [InlineData("./css/site.css")]
    [InlineData("a/b/../c/file.js")]
    [InlineData("a/../b/file.js")]
    [InlineData("")]
    public void NavigatesAboveRoot_ReturnsFalse_ForPathsThatStayWithinRoot(string subpath)
    {
        Assert.False(ModuleStaticFiles.NavigatesAboveRoot(subpath));
    }

    [Theory]
    [InlineData("../secret")]
    [InlineData("..\\secret")]
    [InlineData("../../secret")]
    [InlineData("a/../../secret")]
    [InlineData("a\\..\\..\\secret")]
    [InlineData("..")]
    public void NavigatesAboveRoot_ReturnsTrue_ForPathsThatEscapeRoot(string subpath)
    {
        Assert.True(ModuleStaticFiles.NavigatesAboveRoot(subpath));
    }
}
