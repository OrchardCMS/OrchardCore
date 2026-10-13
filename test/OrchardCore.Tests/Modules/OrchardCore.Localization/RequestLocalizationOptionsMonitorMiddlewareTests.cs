using OrchardCore.Localization;

namespace OrchardCore.Tests.Modules.OrchardCore.Localization;

public class RequestLocalizationOptionsMonitorMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_OptionsChanged_UsesCurrentOptions()
    {
        // Arrange
        var options = CreateOptions("fr-FR");
        var optionsMonitor = new Mock<IOptionsMonitor<RequestLocalizationOptions>>();
        optionsMonitor.SetupGet(monitor => monitor.CurrentValue).Returns(() => options);

        string requestCulture = null;
        var middleware = new RequestLocalizationOptionsMonitorMiddleware(
            context =>
            {
                requestCulture = CultureInfo.CurrentUICulture.Name;
                return Task.CompletedTask;
            },
            optionsMonitor.Object,
            NullLoggerFactory.Instance);

        // Act & Assert
        using (CultureScope.Create("en-US"))
        {
            await middleware.InvokeAsync(new DefaultHttpContext());
            Assert.Equal("fr-FR", requestCulture);

            options = CreateOptions("es-ES");

            await middleware.InvokeAsync(new DefaultHttpContext());
            Assert.Equal("es-ES", requestCulture);
        }
    }

    private static RequestLocalizationOptions CreateOptions(string culture)
    {
        var options = new RequestLocalizationOptions
        {
            // Only the default culture applies.
            RequestCultureProviders = [],
        };

        return options
            .SetDefaultCulture(culture)
            .AddSupportedCultures(culture)
            .AddSupportedUICultures(culture);
    }
}
