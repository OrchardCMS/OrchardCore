using Fluid.Values;
using Microsoft.Extensions.WebEncoders.Testing;
using OrchardCore.ContentManagement;
using OrchardCore.Html.Services;
using OrchardCore.Html.ViewModels;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Shortcodes.Services;
using System.Text.RegularExpressions;

namespace OrchardCore.Tests.Html;

public class HtmlDisplayServiceTests
{
    // An example from https://github.com/OrchardCMS/OrchardCore/issues/19767 for HTML that breaks when sanitized before
    // rendering Liquid first, because it contains a Liquid tag inside the HTML element.
    private const string InputWithLiquidInHtml =
        "<img src=\"{{ \"~/theme/images/logo.png\" | href }}\" class=\"overlay-container\">";
    
    [Theory]
    [InlineData(InputWithLiquidInHtml, true, true, "<img src=\"LIQUID\" class=\"overlay-container\">")]
    [InlineData(InputWithLiquidInHtml, false, false, InputWithLiquidInHtml)]
    public async Task HtmlDisplayService_Processing_Succeeds(
        string html,
        bool renderLiquid,
        bool sanitizeHtml,
        string expected)
    {
        // Arrange
        var service = SetupServices().GetRequiredService<IHtmlDisplayService>();
        var model = new Model(html);
        
        // Act
        await service.UpdateModelHtmlAsync(model, renderLiquid, new Context(), sanitizeHtml);
        var output = model.Html;

        // Assert
        Assert.Equal(expected, output);
    }
    
    [Fact]
    public async Task HtmlDisplayService_Processing_Misconfiguration()
    {
        // Arrange
        var service = SetupServices().GetRequiredService<IHtmlDisplayService>();
        var model = new Model(InputWithLiquidInHtml);
        
        // Act: If the service is misconfigured, it should only fail in the expected way.
        await service.UpdateModelHtmlAsync(model, renderLiquid: false, shortcodeContext: null, sanitizeHtml: true);
        var output = model.Html;

        // Assert
        Assert.Equal("<img src=\"{{ \" href=\"\" class=\"overlay-container\">", output);
    }

    private static ServiceProvider SetupServices()
    {
        var services = new ServiceCollection();

        // Pretend to do Liquid processing, so we don't have to include all dependencies of the template manager.
        var liquidTemplateManagerMock = new Mock<ILiquidTemplateManager>();
        liquidTemplateManagerMock
            .Setup(mock => mock.RenderStringAsync(
                It.IsAny<string>(),
                It.IsAny<HtmlEncoder>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<KeyValuePair<string, FluidValue>>>()))
            .ReturnsAsync<string, HtmlEncoder, object, IEnumerable<KeyValuePair<string, FluidValue>>, ILiquidTemplateManager, string>(
                (template, _, _, _) => Regex.Replace(template, @"\{\{[^{}]+\}\}", "LIQUID"));
        services.AddSingleton(liquidTemplateManagerMock.Object);

        services.AddSingleton<HtmlEncoder>(new HtmlTestEncoder());
        services.AddSingleton<IShortcodeService, ShortcodeService>();

        services.AddOptions<HtmlSanitizerOptions>();
        services.ConfigureHtmlSanitizer(sanitizer => sanitizer.AllowedAttributes.Add("class"));
        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();

        services.AddScoped<IHtmlDisplayService, HtmlDisplayService>();

        return services.BuildServiceProvider();
    }

    private class Model : HtmlViewModelBase
    {
        public Model(string html)
        {
            Html = html;
            ContentItem = new ContentItem();
        }
    }
}
