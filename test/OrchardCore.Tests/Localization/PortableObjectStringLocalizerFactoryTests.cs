using OrchardCore.Localization;
using OrchardCore.Localization.PortableObject;

namespace OrchardCore.Tests.Localization;

public class PortableObjectStringLocalizerFactoryTests
{
    [Fact]
    public async Task LocalizerReturnsTranslationFromInnerClass_Default_Succeeds()
        => await StartupRunner.Run(typeof(PortableObjectStringLocalizerFactoryStartup), "ar", "مرحبا");

    [Fact]
    public void Create_Twice_WithSameType_ReturnsSameLocalizerInstance()
    {
        // Arrange
        var localizationManager = new Mock<ILocalizationManager>();
        var requestLocalizationOptions = Options.Create(new RequestLocalizationOptions{ FallBackToParentUICultures = true });
        var logger = new Mock<ILogger<PortableObjectStringLocalizerFactory>>();
        var factory = new PortableObjectStringLocalizerFactory(localizationManager.Object, requestLocalizationOptions, logger.Object);

        // Act
        var localizer1 = factory.Create(typeof(DummyResource));
        var localizer2 = factory.Create(typeof(DummyResource));

        // Assert
        Assert.Same(localizer1, localizer2);
    }

    [Fact]
    public void Create_WithDifferentTypes_ReturnsDifferentLocalizerInstances()
    {
        // Arrange
        var localizationManager = new Mock<ILocalizationManager>();
        var requestLocalizationOptions = Options.Create(new RequestLocalizationOptions { FallBackToParentUICultures = true });
        var logger = new Mock<ILogger<PortableObjectStringLocalizerFactory>>();
        var factory = new PortableObjectStringLocalizerFactory(localizationManager.Object, requestLocalizationOptions, logger.Object);

        // Act
        var localizer1 = factory.Create(typeof(DummyResource));
        var localizer2 = factory.Create(typeof(AnotherDummyResource));

        // Assert
        Assert.NotSame(localizer1, localizer2);
    }

    public class PortableObjectStringLocalizerFactoryStartup
    {
#pragma warning disable CA1822 // Mark members as static
        public void ConfigureServices(IServiceCollection services)
#pragma warning restore CA1822 // Mark members as static
        {
            services.AddMvc();
            services.AddLocalization();
            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization/PoFiles");
            services.Replace(ServiceDescriptor.Singleton<ILocalizationFileLocationProvider, StubPoFileLocationProvider>());
        }

#pragma warning disable CA1822 // Mark members as static
        public void Configure(
#pragma warning restore CA1822 // Mark members as static
            IApplicationBuilder app,
            IStringLocalizer<Model> localizer)
        {
            var supportedCultures = new[] { "ar", "en" };
            app.UseRequestLocalization(options =>
                options
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures)
                    .SetDefaultCulture("ar")
            );

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync(localizer["Hello"]);
            });
        }
    }

    public class Model
    {
        public string Hello { get; set; }
    }

    private sealed class DummyResource;

    private sealed class AnotherDummyResource;
}
