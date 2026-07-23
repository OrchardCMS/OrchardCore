using Fluid;
using System.Text.Encodings.Web;
using OrchardCore.Liquid;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Liquid;

public class DataProtectionFilterTests
{
    [Fact]
    public async Task EncryptFilter_Default_ReturnsNonEmptyBase64String()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = """{{ "Hello World" | encrypt }}""";
            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template, NullEncoder.Default, null);

            Assert.NotEmpty(result);
            Assert.NotEqual("Hello World", result);
        });
    }

    [Fact]
    public async Task DecryptFilter_Default_ReturnsOriginalValueAfterEncrypt()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = """{{ "Hello World" | encrypt | decrypt }}""";
            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template, NullEncoder.Default, null);

            Assert.Equal("Hello World", result);
        });
    }

    [Fact]
    public async Task DecryptFilter_Default_ReturnsEmptyForEmptyInput()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var template = """{{ "" | decrypt }}""";
            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template, NullEncoder.Default, null);

            Assert.Empty(result);
        });
    }

    [Fact]
    public async Task DecryptFilter_Default_ReturnsNilForInvalidCiphertext()
    {
        var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            // A valid Base64 string that is not a valid ciphertext.
            var invalidCiphertext = Convert.ToBase64String("not-a-valid-ciphertext"u8.ToArray());
            var template = $$$"""{{ "{{{invalidCiphertext}}}" | decrypt }}""";
            var liquidTemplateManager = scope.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var result = await liquidTemplateManager.RenderStringAsync(template, NullEncoder.Default, null);

            Assert.Empty(result.Trim());
        });
    }
}
