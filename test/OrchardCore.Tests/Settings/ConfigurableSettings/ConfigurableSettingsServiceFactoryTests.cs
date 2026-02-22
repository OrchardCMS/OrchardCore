using Microsoft.Extensions.Configuration;
using Moq;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings;
using OrchardCore.Settings.Services;

namespace OrchardCore.Tests.Settings.ConfigurableSettings;

public class ConfigurableSettingsServiceFactoryTests
{
    #region Test Settings Class

    public class TestSettings : IConfigurableSettings
    {
        public bool DisableUIConfiguration { get; set; }

        [ConfigurationProperty(MergeStrategy = PropertyMergeStrategy.FileOverridesDatabase)]
        public string Setting1 { get; set; }

        [ConfigurationProperty(MergeStrategy = PropertyMergeStrategy.Merge)]
        public string[] ArraySetting { get; set; } = [];
    }

    #endregion

    private static IShellConfiguration CreateMockShellConfiguration(Dictionary<string, string> configValues)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var mockShellConfiguration = new Mock<IShellConfiguration>();
        mockShellConfiguration
            .Setup(x => x.GetSection(It.IsAny<string>()))
            .Returns((string key) => configuration.GetSection(key));

        return mockShellConfiguration.Object;
    }

    #region GetFileConfigurationSettings Tests

    [Fact]
    public void GetFileConfigurationSettings_ReturnsSettingsFromConfiguration()
    {
        // Arrange
        var configValues = new Dictionary<string, string>
        {
            ["TestModule:Setting1"] = "file-value",
        };
        var shellConfiguration = CreateMockShellConfiguration(configValues);
        var mergeStrategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        var factory = new ConfigurableSettingsServiceFactory<TestSettings>(
            shellConfiguration,
            mergeStrategy,
            "TestModule");

        // Act
        var result = factory.GetFileConfigurationSettings();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("file-value", result.Setting1);
    }

    [Fact]
    public void GetFileConfigurationSettings_CachesResult()
    {
        // Arrange
        var configValues = new Dictionary<string, string>
        {
            ["TestModule:Setting1"] = "file-value",
        };
        var shellConfiguration = CreateMockShellConfiguration(configValues);
        var mergeStrategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        var factory = new ConfigurableSettingsServiceFactory<TestSettings>(
            shellConfiguration,
            mergeStrategy,
            "TestModule");

        // Act
        var result1 = factory.GetFileConfigurationSettings();
        var result2 = factory.GetFileConfigurationSettings();

        // Assert
        Assert.Same(result1, result2);
    }

    [Fact]
    public void GetFileConfigurationSettings_WithEmptyConfigKey_ReturnsDefaultSettings()
    {
        // Arrange
        var configValues = new Dictionary<string, string>();
        var shellConfiguration = CreateMockShellConfiguration(configValues);
        var mergeStrategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        var factory = new ConfigurableSettingsServiceFactory<TestSettings>(
            shellConfiguration,
            mergeStrategy,
            "");

        // Act
        var result = factory.GetFileConfigurationSettings();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Setting1);
    }

    [Fact]
    public void GetFileConfigurationSettings_WithNonExistentKey_ReturnsDefaultSettings()
    {
        // Arrange
        var configValues = new Dictionary<string, string>
        {
            ["OtherModule:Setting1"] = "file-value",
        };
        var shellConfiguration = CreateMockShellConfiguration(configValues);
        var mergeStrategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        var factory = new ConfigurableSettingsServiceFactory<TestSettings>(
            shellConfiguration,
            mergeStrategy,
            "TestModule");

        // Act
        var result = factory.GetFileConfigurationSettings();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Setting1);
    }

    #endregion

    #region MergeSettings Tests

    [Fact]
    public void MergeSettings_MergesDatabaseAndFileSettings()
    {
        // Arrange
        var configValues = new Dictionary<string, string>
        {
            ["TestModule:Setting1"] = "file-value",
        };
        var shellConfiguration = CreateMockShellConfiguration(configValues);
        var mergeStrategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        var factory = new ConfigurableSettingsServiceFactory<TestSettings>(
            shellConfiguration,
            mergeStrategy,
            "TestModule");

        var dbSettings = new TestSettings { Setting1 = "db-value" };

        // Act
        var result = factory.MergeSettings(dbSettings);

        // Assert
        // FileOverridesDatabase strategy, so file value wins
        Assert.Equal("file-value", result.Setting1);
    }

    [Fact]
    public void MergeSettings_WhenNoFileValue_UsesDatabaseValue()
    {
        // Arrange
        var configValues = new Dictionary<string, string>();
        var shellConfiguration = CreateMockShellConfiguration(configValues);
        var mergeStrategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        var factory = new ConfigurableSettingsServiceFactory<TestSettings>(
            shellConfiguration,
            mergeStrategy,
            "TestModule");

        var dbSettings = new TestSettings { Setting1 = "db-value" };

        // Act
        var result = factory.MergeSettings(dbSettings);

        // Assert
        Assert.Equal("db-value", result.Setting1);
    }

    [Fact]
    public void MergeSettings_WithMergeStrategy_CombinesArrays()
    {
        // Arrange
        var configValues = new Dictionary<string, string>
        {
            ["TestModule:ArraySetting:0"] = "file-item1",
            ["TestModule:ArraySetting:1"] = "file-item2",
        };
        var shellConfiguration = CreateMockShellConfiguration(configValues);
        var mergeStrategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        var factory = new ConfigurableSettingsServiceFactory<TestSettings>(
            shellConfiguration,
            mergeStrategy,
            "TestModule");

        var dbSettings = new TestSettings { ArraySetting = ["db-item1", "file-item1"] };

        // Act
        var result = factory.MergeSettings(dbSettings);

        // Assert
        Assert.Contains("file-item1", result.ArraySetting);
        Assert.Contains("file-item2", result.ArraySetting);
        Assert.Contains("db-item1", result.ArraySetting);
    }

    #endregion

    #region IsUIConfigurationDisabled Tests

    [Fact]
    public void IsUIConfigurationDisabled_WhenTrue_ReturnsTrue()
    {
        // Arrange
        var configValues = new Dictionary<string, string>
        {
            ["TestModule:DisableUIConfiguration"] = "true",
        };
        var shellConfiguration = CreateMockShellConfiguration(configValues);
        var mergeStrategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        var factory = new ConfigurableSettingsServiceFactory<TestSettings>(
            shellConfiguration,
            mergeStrategy,
            "TestModule");

        // Act
        var result = factory.IsUIConfigurationDisabled;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsUIConfigurationDisabled_WhenFalse_ReturnsFalse()
    {
        // Arrange
        var configValues = new Dictionary<string, string>
        {
            ["TestModule:DisableUIConfiguration"] = "false",
        };
        var shellConfiguration = CreateMockShellConfiguration(configValues);
        var mergeStrategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        var factory = new ConfigurableSettingsServiceFactory<TestSettings>(
            shellConfiguration,
            mergeStrategy,
            "TestModule");

        // Act
        var result = factory.IsUIConfigurationDisabled;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsUIConfigurationDisabled_WhenNotSet_ReturnsFalse()
    {
        // Arrange
        var configValues = new Dictionary<string, string>();
        var shellConfiguration = CreateMockShellConfiguration(configValues);
        var mergeStrategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        var factory = new ConfigurableSettingsServiceFactory<TestSettings>(
            shellConfiguration,
            mergeStrategy,
            "TestModule");

        // Act
        var result = factory.IsUIConfigurationDisabled;

        // Assert
        Assert.False(result);
    }

    #endregion
}
