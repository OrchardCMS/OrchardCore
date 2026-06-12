using Fluid;
using OrchardCore.Liquid;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Liquid;

public class SlugifyFilterTests
{
    [Theory]
    [InlineData("Æneid Æneid", "æneid-æneid")]
    [InlineData("Aeneid Aeneid", "aeneid-aeneid")]
    public async Task SlugifyFilterShouldReturnSlugifiedString(string text, string expected)
    {
        // Arrange
        var context = new SiteContext();

        await context.InitializeAsync();

        // Act & Assert
        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = $$$"""{{ "{{{text}}}" | slugify }}""";

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();

            var result = await liquidTemplateManager.RenderStringAsync(template, NullEncoder.Default, null);

            Assert.NotEmpty(result);
            Assert.Equal(expected, result);
        });
    }

    [Fact]
    public async Task SlugifyThenTransliterateFilterShouldReturnSlugifiedTransliteratedString()
    {
        // Arrange
        var context = new SiteContext();

        await context.InitializeAsync();

        // Act & Assert
        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = """{{ "Æneid Æneid" | transliterate | slugify }}""";

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();

            var result = await liquidTemplateManager.RenderStringAsync(template, NullEncoder.Default, null);

            Assert.NotEmpty(result);
            Assert.Equal("aeneid-aeneid", result);
        });
    }
}
