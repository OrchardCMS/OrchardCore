using OrchardCore.Localization.Data;

namespace OrchardCore.Localization.Tests;

public class LocalizationServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDataLocaliztion_RegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddMemoryCache();
        services.AddSingleton<ILocalizationManager, LocalizationManager>();

        // Act
        services.AddDataLocalization();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var translationProvider = serviceProvider.GetService<IDataTranslationProvider>();
        var dataResourceManager = serviceProvider.GetService<DataResourceManager>();
        var dataLocalizerFactory = serviceProvider.GetService<IDataLocalizerFactory>();
        var dataLocalizer = serviceProvider.GetService<IDataLocalizer>();

        Assert.NotNull(translationProvider);
        Assert.NotNull(dataResourceManager);
        Assert.NotNull(dataLocalizerFactory);
        Assert.NotNull(dataLocalizer);
    }

    [Fact]
    public void AddPortableObjectLocalization_RegistersNullDataLocalizer()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddMemoryCache();
        services.AddSingleton<IPluralRuleProvider, DefaultPluralRuleProvider>();

        // Act
        services.AddPortableObjectLocalization();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dataLocalizerFactory = serviceProvider.GetService<IDataLocalizerFactory>();
        var dataLocalizer = serviceProvider.GetService<IDataLocalizer>();

        Assert.NotNull(dataLocalizerFactory);
        Assert.NotNull(dataLocalizer);

        // Verify it returns the original value (no translation)
        var result = dataLocalizer["TestName", "TestContext"];
        Assert.Equal("TestName", result.Name);
        Assert.Equal("TestContext", result.Context);
        Assert.Equal("TestName", result.Value);
        Assert.True(result.ResourceNotFound);
    }

    [Fact]
    public void AddDataLocalization_ReplacesNullDataLocalizer()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddMemoryCache();
        services.AddSingleton<ILocalizationManager, LocalizationManager>();

        // Act - First add PortableObject localization (which registers null data localizer)
        services.AddPortableObjectLocalization();
        // Then add DataLocalization (which should replace it)
        services.AddDataLocalization();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dataLocalizerFactory = serviceProvider.GetService<IDataLocalizerFactory>();
        var dataLocalizer = serviceProvider.GetService<IDataLocalizer>();

        Assert.NotNull(dataLocalizerFactory);
        Assert.NotNull(dataLocalizer);
        
        // Verify it's the actual DataLocalizerFactory, not NullDataLocalizerFactory
        Assert.IsType<DataLocalizerFactory>(dataLocalizerFactory);
    }
}
