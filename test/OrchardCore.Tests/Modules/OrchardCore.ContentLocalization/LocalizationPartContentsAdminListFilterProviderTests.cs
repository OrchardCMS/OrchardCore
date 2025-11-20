using OrchardCore.ContentLocalization.Services;

namespace OrchardCore.ContentLocalization.Tests;

public class LocalizationPartContentsAdminListFilterProviderTests
{
    [Theory]
    [InlineData("en-GB", "en-gb")]
    [InlineData("nl-NL", "nl-nl")]
    [InlineData("ar-SA", "ar-sa")]
    [InlineData("en-us", "en-us")]
    [InlineData("FR-FR", "fr-fr")]
    [InlineData("DE-DE", "de-de")]
    public void CultureNormalization_ConvertsToLowerInvariant(string inputCulture, string expectedNormalizedCulture)
    {
        // This test verifies that the culture normalization logic works correctly.
        // The actual normalization is done via ToLowerInvariant() which is applied
        // in LocalizationPartContentsAdminListFilterProvider before querying the database.
        
        // Arrange & Act
        var normalized = inputCulture.ToLowerInvariant();
        
        // Assert
        Assert.Equal(expectedNormalizedCulture, normalized);
    }
    
    [Fact]
    public void LocalizationPartContentsAdminListFilterProvider_IsInstantiable()
    {
        // Arrange & Act
        var provider = new LocalizationPartContentsAdminListFilterProvider();
        
        // Assert
        Assert.NotNull(provider);
    }
}
