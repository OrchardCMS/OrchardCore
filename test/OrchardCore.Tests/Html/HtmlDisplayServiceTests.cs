using OrchardCore.ContentManagement;
using OrchardCore.Html.Services;
using OrchardCore.Html.ViewModels;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Tests.Html;

public class HtmlDisplayServiceTests
{
    [Theory]
    [InlineData("<script>alert('xss')</script><p>Safe</p>", true, "<p>Safe</p>")]
    [InlineData("<script>alert('xss')</script><p>Safe</p>", false, "<script>alert('xss')</script><p>Safe</p>")]
    public async Task HtmlDisplayService_Processing_Succeeds(
        string html,
        bool sanitizeHtml,
        string expected)
    {
        // Arrange
        var service = SetupServices().GetRequiredService<IHtmlDisplayService>();
        var model = new Model(html);

        // Act
        await service.UpdateModelHtmlAsync(model, new Context(), sanitizeHtml);
        var output = model.Html;

        // Assert
        Assert.Equal(expected, output);
    }

    [Fact]
    public async Task HtmlDisplayService_LiquidSyntax_IsNotExecuted()
    {
        // Arrange
        var service = SetupServices().GetRequiredService<IHtmlDisplayService>();
        const string html = "<p>{{ ContentItem.DisplayText }}</p>";
        var model = new Model(html);

        await service.UpdateModelHtmlAsync(model, shortcodeContext: null, sanitizeHtml: false);
        var output = model.Html;

        Assert.Equal(html, output);
    }

    [Theory]
    [InlineData(true, "<p>Safe</p>")]
    [InlineData(false, "<script>alert('xss')</script><p>Safe</p>")]
    public async Task HtmlDisplayService_SanitizesAfterShortcodes(
        bool sanitizeHtml,
        string expected)
    {
        var service = SetupServices(new UnsafeShortcodeService()).GetRequiredService<IHtmlDisplayService>();
        var model = new Model("[unsafe]<p>Safe</p>");

        await service.UpdateModelHtmlAsync(model, new Context(), sanitizeHtml);

        Assert.Equal(expected, model.Html);
    }

    private static ServiceProvider SetupServices(IShortcodeService shortcodeService = null)
    {
        var services = new ServiceCollection();

        services.AddSingleton(shortcodeService ?? new PassthroughShortcodeService());

        services.AddOptions<HtmlSanitizerOptions>();
        services.ConfigureHtmlSanitizer(sanitizer => sanitizer.AllowedAttributes.Add("class"));
        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();

        services.AddScoped<IHtmlDisplayService, HtmlDisplayService>();

        return services.BuildServiceProvider();
    }

    private sealed class UnsafeShortcodeService : IShortcodeService
    {
        public ValueTask<string> ProcessAsync(string input, Context context = null)
            => ValueTask.FromResult(input.Replace("[unsafe]", "<script>alert('xss')</script>"));
    }

    private sealed class PassthroughShortcodeService : IShortcodeService
    {
        public ValueTask<string> ProcessAsync(string input, Context context = null)
            => ValueTask.FromResult(input);
    }

    private sealed class Model : HtmlViewModelBase
    {
        public Model(string html)
        {
            Html = html;
            ContentItem = new ContentItem();
        }
    }
}
