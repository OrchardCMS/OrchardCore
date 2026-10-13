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
        var requestLocalizationOptions = Mock.Of<IOptionsMonitor<RequestLocalizationOptions>>(monitor => monitor.CurrentValue == new RequestLocalizationOptions { FallBackToParentUICultures = true });
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
        var requestLocalizationOptions = Mock.Of<IOptionsMonitor<RequestLocalizationOptions>>(monitor => monitor.CurrentValue == new RequestLocalizationOptions { FallBackToParentUICultures = true });
        var logger = new Mock<ILogger<PortableObjectStringLocalizerFactory>>();
        var factory = new PortableObjectStringLocalizerFactory(localizationManager.Object, requestLocalizationOptions, logger.Object);

        // Act
        var localizer1 = factory.Create(typeof(DummyResource));
        var localizer2 = factory.Create(typeof(AnotherDummyResource));

        // Assert
        Assert.NotSame(localizer1, localizer2);
    }

    [Theory]
    [InlineData("OrchardCore.Tests.Localization.Model")]
    [InlineData("OrchardCore.Tests.Localization.Outer+Inner")]
    public void Create_Twice_WithSameBaseNameAndLocation_ReturnsSameLocalizerInstance(string baseName)
    {
        // Arrange
        var localizationManager = new Mock<ILocalizationManager>();
        var requestLocalizationOptions = Mock.Of<IOptionsMonitor<RequestLocalizationOptions>>(monitor => monitor.CurrentValue == new RequestLocalizationOptions { FallBackToParentUICultures = true });
        var logger = new Mock<ILogger<PortableObjectStringLocalizerFactory>>();
        var factory = new PortableObjectStringLocalizerFactory(localizationManager.Object, requestLocalizationOptions, logger.Object);

        // Act
        var localizer1 = factory.Create(baseName, "OrchardCore.Tests");
        var localizer2 = factory.Create(baseName, "OrchardCore.Tests");

        // Assert
        Assert.Same(localizer1, localizer2);
    }

    [Fact]
    public void Create_FallBackToParentCulturesChanged_UsesCurrentValue()
    {
        // Arrange
        var dictionary = new CultureDictionary("fr", PluralizationRule.English);
        dictionary.MergeTranslations([new CultureDictionaryRecord("Hello", null, ["Bonjour"])]);

        var localizationManager = new Mock<ILocalizationManager>();
        localizationManager
            .Setup(manager => manager.GetDictionary(It.Is<CultureInfo>(culture => culture.Name == "fr")))
            .Returns(dictionary);

        var fallBackToParentCultures = false;
        var changeTokenSource = new RequestLocalizationOptionsChangeTokenSource();

        using var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(localizationManager.Object)
            .AddSingleton<IOptionsChangeTokenSource<RequestLocalizationOptions>>(changeTokenSource)
            .Configure<RequestLocalizationOptions>(options => options.FallBackToParentUICultures = fallBackToParentCultures)
            .AddSingleton<IStringLocalizerFactory, PortableObjectStringLocalizerFactory>()
            .BuildServiceProvider();

        var factory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();

        using var cultureScope = CultureScope.Create("fr-CA");

        Assert.Equal("Hello", factory.Create(typeof(DummyResource))["Hello"].Value);

        // Act
        fallBackToParentCultures = true;
        changeTokenSource.Change();

        // Assert
        Assert.Equal("Bonjour", factory.Create(typeof(DummyResource))["Hello"].Value);
        Assert.Equal("Bonjour", factory.Create(typeof(DummyResource).FullName, "OrchardCore.Tests")["Hello"].Value);
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

    private sealed class RequestLocalizationOptionsChangeTokenSource : IOptionsChangeTokenSource<RequestLocalizationOptions>
    {
        private CancellationTokenSource _cancellationTokenSource = new();

        public string Name => Options.DefaultName;

        public IChangeToken GetChangeToken() => new CancellationChangeToken(_cancellationTokenSource.Token);

        public void Change()
        {
            var previous = Interlocked.Exchange(ref _cancellationTokenSource, new CancellationTokenSource());
            previous.Cancel();
        }
    }
}
