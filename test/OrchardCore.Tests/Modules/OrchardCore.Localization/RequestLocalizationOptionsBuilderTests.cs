using Microsoft.AspNetCore.Localization;

namespace OrchardCore.Modules.OrchardCore.Localization.Tests;

public class RequestLocalizationOptionsBuilderTests
{
    [Fact]
    public void ShouldBuildLocalizationOptions()
    {
        // Arrange
        var cultures = new[] { "ar", "en", "fr" };
        var builder = new RequestLocalizationOptionsBuilder(new RequestLocalizationOptions(), useUserOverride: false);

        // Act
        builder
            .AddSupportedCultures(cultures)
            .AddSupportedUICultures(cultures)
            .SetDefaultCulture(cultures[0]);

        var options = builder.Options;

        // Assert
        Assert.Equal(3, options.SupportedCultures.Count);
        Assert.Equal(3, options.SupportedUICultures.Count);
        Assert.Contains(options.SupportedCultures, c => cultures.Contains(c.Name));
        Assert.Contains(options.SupportedUICultures, c => cultures.Contains(c.Name));
        Assert.Equal(cultures[0], options.DefaultRequestCulture.Culture.Name);
        Assert.True(options.SupportedCultures.All(c => c.UseUserOverride == false));
    }

    [Fact]
    public void ShouldNotAvoidPreviousOptions()
    {
        // Arrange
        var cultures = new[] { "ar", "en", "fr" };
        var oldOptions = new RequestLocalizationOptions();
        oldOptions.AddInitialRequestCultureProvider(new CustomRequestCultureProvider());

        var builder = new RequestLocalizationOptionsBuilder(oldOptions, useUserOverride: false);

        // Act
        builder
            .AddSupportedCultures(cultures)
            .AddSupportedUICultures(cultures)
            .SetDefaultCulture(cultures[0]);

        var newOptions = builder.Options;

        // Assert
        Assert.Equal(3, newOptions.SupportedCultures.Count);
        Assert.Equal(3, newOptions.SupportedUICultures.Count);
        Assert.Equal(cultures[0], newOptions.DefaultRequestCulture.Culture.Name);
        Assert.Equal(4, newOptions.RequestCultureProviders.Count);
        Assert.Equal(nameof(CustomRequestCultureProvider), newOptions.RequestCultureProviders.First().GetType().Name);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SupportedCulturesShouldRespectUseUserOverride(bool useUserOverride)
    {
        // Arrange
        var cultures = new[] { "ar", "en", "fr" };
        var oldOptions = new RequestLocalizationOptions();
        var builder = new RequestLocalizationOptionsBuilder(oldOptions, useUserOverride);

        // Act
        builder
            .AddSupportedCultures(cultures)
            .AddSupportedUICultures(cultures)
            .SetDefaultCulture(cultures[0]);

        var newOptions = builder.Options;

        // Assert
        Assert.True(newOptions.SupportedCultures.All(c => c.UseUserOverride == useUserOverride));
    }

    private class CustomRequestCultureProvider : RequestCultureProvider
    {
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            throw new NotImplementedException();
        }
    }
}
