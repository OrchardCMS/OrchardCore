using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Tests.Modules.OrchardCore.Media.Azure;

public class BlobContainerNameValidatorTests
{
    [Theory]
    [InlineData("abc")]                                          // minimum length
    [InlineData("media")]
    [InlineData("tenant1-media")]
    [InlineData("a1-b2-c3")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // 63 chars
    public void Valid_Default_Succeeds(string name)
        => Assert.True(BlobContainerNameValidator.IsValid(name));

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("ab")]                              // too short
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // 64 chars
    [InlineData("Default-Media")]                   // uppercase
    [InlineData("default_imagecache")]              // underscore
    [InlineData("media.cache")]                     // period
    [InlineData("-leading")]                        // starts with hyphen
    [InlineData("trailing-")]                       // ends with hyphen
    [InlineData("double--hyphen")]                  // consecutive hyphens
    public void Invalid_Default_Succeeds(string name)
        => Assert.False(BlobContainerNameValidator.IsValid(name));
}
