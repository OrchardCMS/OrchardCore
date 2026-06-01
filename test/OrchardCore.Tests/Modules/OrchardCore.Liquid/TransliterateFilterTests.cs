using Fluid;
using OrchardCore.Liquid;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Liquid;

public class TransliterateFilterTests
{
    [Theory]
    [InlineData("Æneid", "Aeneid")]
    [InlineData("æneid", "aeneid")]
    [InlineData("Aeneid", "Aeneid")]
    [InlineData("aeneid", "aeneid")]
    [InlineData("Ελληνικά", "Ellinika")]
    [InlineData("джинсы_клеш", "dzhinsy_klesh")]
    public async Task TransliterateFilterShouldReturnTransliteratedString(string text, string expected)
    {
        // Arrange
        var context = new SiteContext();

        await context.InitializeAsync();

        // Act & Assert
        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = $$$"""{{ "{{{text}}}" | transliterate }}""";

            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();

            var result = await liquidTemplateManager.RenderStringAsync(template, NullEncoder.Default, null);

            Assert.NotEmpty(result);
            Assert.Equal(expected, result);
        });
    }
}
