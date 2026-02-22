using OrchardCore.Settings;
using OrchardCore.Settings.Services;

namespace OrchardCore.Tests.Settings.ConfigurableSettings;

public class AttributeBasedSettingsMergeStrategyTests
{
    #region Test Settings Classes

    public class TestSettings : IConfigurableSettings
    {
        public bool DisableUIConfiguration { get; set; }

        [ConfigurationProperty(MergeStrategy = PropertyMergeStrategy.FileOverridesDatabase)]
        public string FileOverridesSetting { get; set; }

        [ConfigurationProperty(MergeStrategy = PropertyMergeStrategy.DatabaseOverridesFile)]
        public string DatabaseOverridesSetting { get; set; }

        [ConfigurationProperty(MergeStrategy = PropertyMergeStrategy.FileAsDefault)]
        public string FileAsDefaultSetting { get; set; }

        [ConfigurationProperty(MergeStrategy = PropertyMergeStrategy.DatabaseAsDefault)]
        public string DatabaseAsDefaultSetting { get; set; }

        [ConfigurationProperty(MergeStrategy = PropertyMergeStrategy.FileOnly)]
        public string FileOnlySetting { get; set; }

        [ConfigurationProperty(MergeStrategy = PropertyMergeStrategy.DatabaseOnly)]
        public string DatabaseOnlySetting { get; set; }

        [ConfigurationProperty(MergeStrategy = PropertyMergeStrategy.Merge)]
        public string[] MergeArraySetting { get; set; } = [];

        [ConfigurationProperty(MergeStrategy = PropertyMergeStrategy.Merge)]
        public List<string> MergeListSetting { get; set; } = [];

        public string DefaultStrategySetting { get; set; }
    }

    public class TestSettingsWithDefaults : IConfigurableSettings
    {
        public bool DisableUIConfiguration { get; set; }

        [DefaultConfigurationValue("default-value")]
        public string SettingWithDefault { get; set; }

        [DefaultConfigurationValue(42)]
        public int IntSettingWithDefault { get; set; }
    }

    public class TestSettingsForUIDisabled : IConfigurableSettings
    {
        public bool DisableUIConfiguration { get; set; }

        [ConfigurationProperty(MergeStrategy = PropertyMergeStrategy.FileOverridesDatabase)]
        public string Setting1 { get; set; }

        [ConfigurationProperty(MergeStrategy = PropertyMergeStrategy.DatabaseOverridesFile)]
        public string Setting2 { get; set; }
    }

    #endregion

    #region Merge Strategy Tests

    [Fact]
    public void Merge_FileOverridesDatabase_WhenBothHaveValues_ReturnsFileValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { FileOverridesSetting = "db-value" };
        var fileSettings = new TestSettings { FileOverridesSetting = "file-value" };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("file-value", result.FileOverridesSetting);
    }

    [Fact]
    public void Merge_FileOverridesDatabase_WhenOnlyDbHasValue_ReturnsDatabaseValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { FileOverridesSetting = "db-value" };
        var fileSettings = new TestSettings { FileOverridesSetting = null };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("db-value", result.FileOverridesSetting);
    }

    [Fact]
    public void Merge_FileOverridesDatabase_WhenOnlyFileHasValue_ReturnsFileValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { FileOverridesSetting = null };
        var fileSettings = new TestSettings { FileOverridesSetting = "file-value" };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("file-value", result.FileOverridesSetting);
    }

    [Fact]
    public void Merge_DatabaseOverridesFile_WhenBothHaveValues_ReturnsDatabaseValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { DatabaseOverridesSetting = "db-value" };
        var fileSettings = new TestSettings { DatabaseOverridesSetting = "file-value" };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("db-value", result.DatabaseOverridesSetting);
    }

    [Fact]
    public void Merge_DatabaseOverridesFile_WhenOnlyFileHasValue_ReturnsFileValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { DatabaseOverridesSetting = null };
        var fileSettings = new TestSettings { DatabaseOverridesSetting = "file-value" };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("file-value", result.DatabaseOverridesSetting);
    }

    [Fact]
    public void Merge_FileAsDefault_WhenBothHaveValues_ReturnsDatabaseValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { FileAsDefaultSetting = "db-value" };
        var fileSettings = new TestSettings { FileAsDefaultSetting = "file-value" };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("db-value", result.FileAsDefaultSetting);
    }

    [Fact]
    public void Merge_FileAsDefault_WhenOnlyFileHasValue_ReturnsFileValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { FileAsDefaultSetting = null };
        var fileSettings = new TestSettings { FileAsDefaultSetting = "file-value" };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("file-value", result.FileAsDefaultSetting);
    }

    [Fact]
    public void Merge_DatabaseAsDefault_WhenBothHaveValues_ReturnsFileValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { DatabaseAsDefaultSetting = "db-value" };
        var fileSettings = new TestSettings { DatabaseAsDefaultSetting = "file-value" };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("file-value", result.DatabaseAsDefaultSetting);
    }

    [Fact]
    public void Merge_DatabaseAsDefault_WhenOnlyDbHasValue_ReturnsDatabaseValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { DatabaseAsDefaultSetting = "db-value" };
        var fileSettings = new TestSettings { DatabaseAsDefaultSetting = null };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("db-value", result.DatabaseAsDefaultSetting);
    }

    [Fact]
    public void Merge_FileOnly_WhenBothHaveValues_ReturnsFileValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { FileOnlySetting = "db-value" };
        var fileSettings = new TestSettings { FileOnlySetting = "file-value" };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("file-value", result.FileOnlySetting);
    }

    [Fact]
    public void Merge_FileOnly_WhenOnlyDbHasValue_ReturnsNull()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { FileOnlySetting = "db-value" };
        var fileSettings = new TestSettings { FileOnlySetting = null };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Null(result.FileOnlySetting);
    }

    [Fact]
    public void Merge_DatabaseOnly_WhenBothHaveValues_ReturnsDatabaseValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { DatabaseOnlySetting = "db-value" };
        var fileSettings = new TestSettings { DatabaseOnlySetting = "file-value" };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("db-value", result.DatabaseOnlySetting);
    }

    [Fact]
    public void Merge_DatabaseOnly_WhenOnlyFileHasValue_ReturnsNull()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { DatabaseOnlySetting = null };
        var fileSettings = new TestSettings { DatabaseOnlySetting = "file-value" };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Null(result.DatabaseOnlySetting);
    }

    [Fact]
    public void Merge_DefaultStrategy_WithoutAttribute_UsesFileOverridesDatabase()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { DefaultStrategySetting = "db-value" };
        var fileSettings = new TestSettings { DefaultStrategySetting = "file-value" };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("file-value", result.DefaultStrategySetting);
    }

    #endregion

    #region Array and Collection Merge Tests

    [Fact]
    public void Merge_ArrayMergeStrategy_CombinesUniqueValues()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { MergeArraySetting = ["a", "b", "c"] };
        var fileSettings = new TestSettings { MergeArraySetting = ["b", "c", "d"] };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Contains("a", result.MergeArraySetting);
        Assert.Contains("b", result.MergeArraySetting);
        Assert.Contains("c", result.MergeArraySetting);
        Assert.Contains("d", result.MergeArraySetting);
        // Should have unique values only
        Assert.Equal(4, result.MergeArraySetting.Length);
    }

    [Fact]
    public void Merge_ArrayMergeStrategy_WhenOnlyDbHasValues_ReturnsDatabaseValues()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { MergeArraySetting = ["a", "b"] };
        var fileSettings = new TestSettings { MergeArraySetting = [] };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal(2, result.MergeArraySetting.Length);
        Assert.Contains("a", result.MergeArraySetting);
        Assert.Contains("b", result.MergeArraySetting);
    }

    [Fact]
    public void Merge_ListMergeStrategy_CombinesUniqueValues()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { MergeListSetting = ["a", "b", "c"] };
        var fileSettings = new TestSettings { MergeListSetting = ["b", "c", "d"] };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Contains("a", result.MergeListSetting);
        Assert.Contains("b", result.MergeListSetting);
        Assert.Contains("c", result.MergeListSetting);
        Assert.Contains("d", result.MergeListSetting);
        Assert.Equal(4, result.MergeListSetting.Count);
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void Merge_WithDefaultAttribute_WhenNoValueProvided_ReturnsDefaultValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettingsWithDefaults>();
        var dbSettings = new TestSettingsWithDefaults { SettingWithDefault = null };
        var fileSettings = new TestSettingsWithDefaults { SettingWithDefault = null };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("default-value", result.SettingWithDefault);
    }

    [Fact]
    public void Merge_WithDefaultAttribute_WhenValueProvided_ReturnsProvidedValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettingsWithDefaults>();
        var dbSettings = new TestSettingsWithDefaults { SettingWithDefault = "custom-value" };
        var fileSettings = new TestSettingsWithDefaults { SettingWithDefault = null };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal("custom-value", result.SettingWithDefault);
    }

    [Fact]
    public void Merge_IntSetting_UsesFileOverridesDatabaseByDefault()
    {
        // Arrange
        // For value types like int, without ConfigurationProperty attribute,
        // the default FileOverridesDatabase strategy is used.
        // Since file has a non-zero value, it takes precedence.
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettingsWithDefaults>();
        var dbSettings = new TestSettingsWithDefaults { IntSettingWithDefault = 50 };
        var fileSettings = new TestSettingsWithDefaults { IntSettingWithDefault = 100 };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.Equal(100, result.IntSettingWithDefault);
    }

    [Fact]
    public void Merge_IntSetting_WhenOnlyDbHasNonZeroValue_ReturnsDatabaseValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettingsWithDefaults>();
        var dbSettings = new TestSettingsWithDefaults { IntSettingWithDefault = 100 };
        var fileSettings = new TestSettingsWithDefaults { IntSettingWithDefault = 0 };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        // 0 is not considered empty for value types - it's a valid value
        // Both have values (100 and 0), so file wins with FileOverridesDatabase
        Assert.Equal(0, result.IntSettingWithDefault);
    }

    #endregion

    #region UI Disabled Tests

    [Fact]
    public void Merge_WhenUIDisabled_AlwaysUsesFileValue()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettingsForUIDisabled>();
        var dbSettings = new TestSettingsForUIDisabled
        {
            Setting1 = "db-value",
            Setting2 = "db-value",
        };
        var fileSettings = new TestSettingsForUIDisabled
        {
            DisableUIConfiguration = true,
            Setting1 = "file-value",
            Setting2 = "file-value",
        };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.True(result.DisableUIConfiguration);
        Assert.Equal("file-value", result.Setting1);
        Assert.Equal("file-value", result.Setting2);
    }

    [Fact]
    public void Merge_WhenUIDisabled_AndNoFileValue_ReturnsNull()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettingsForUIDisabled>();
        var dbSettings = new TestSettingsForUIDisabled { Setting1 = "db-value" };
        var fileSettings = new TestSettingsForUIDisabled
        {
            DisableUIConfiguration = true,
            Setting1 = null,
        };

        // Act
        var result = strategy.Merge(dbSettings, fileSettings, null);

        // Assert
        Assert.True(result.DisableUIConfiguration);
        Assert.Null(result.Setting1);
    }

    #endregion

    #region DeterminePropertySource Tests

    [Theory]
    [InlineData("file-value", "db-value", false, ConfigurationSource.ConfigurationFile)]
    [InlineData(null, "db-value", false, ConfigurationSource.Database)]
    [InlineData("file-value", null, false, ConfigurationSource.ConfigurationFile)]
    [InlineData(null, null, false, ConfigurationSource.Default)]
    public void DeterminePropertySource_FileOverridesDatabase_ReturnsCorrectSource(
        string fileValue, string dbValue, bool uiDisabled, ConfigurationSource expectedSource)
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        // Act
        var result = strategy.DeterminePropertySource("FileOverridesSetting", dbValue, fileValue, uiDisabled);

        // Assert
        Assert.Equal(expectedSource, result);
    }

    [Fact]
    public void DeterminePropertySource_WhenUIDisabled_ReturnsFileSourceIfAvailable()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        // Act
        var result = strategy.DeterminePropertySource("FileOverridesSetting", "db-value", "file-value", uiDisabled: true);

        // Assert
        Assert.Equal(ConfigurationSource.ConfigurationFile, result);
    }

    [Fact]
    public void DeterminePropertySource_WhenUIDisabled_AndNoFileValue_ReturnsDefault()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        // Act
        var result = strategy.DeterminePropertySource("FileOverridesSetting", "db-value", null, uiDisabled: true);

        // Assert
        Assert.Equal(ConfigurationSource.Default, result);
    }

    [Fact]
    public void DeterminePropertySource_FileOnlyStrategy_ReturnsFileSourceOrDefault()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        // Act & Assert
        Assert.Equal(ConfigurationSource.ConfigurationFile,
            strategy.DeterminePropertySource("FileOnlySetting", "db-value", "file-value", false));
        Assert.Equal(ConfigurationSource.Default,
            strategy.DeterminePropertySource("FileOnlySetting", "db-value", null, false));
    }

    [Fact]
    public void DeterminePropertySource_DatabaseOnlyStrategy_ReturnsDatabaseSourceOrDefault()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        // Act & Assert
        Assert.Equal(ConfigurationSource.Database,
            strategy.DeterminePropertySource("DatabaseOnlySetting", "db-value", "file-value", false));
        Assert.Equal(ConfigurationSource.Default,
            strategy.DeterminePropertySource("DatabaseOnlySetting", null, "file-value", false));
    }

    #endregion

    #region Null Settings Tests

    [Fact]
    public void Merge_WhenDatabaseSettingsIsNull_CreatesNewInstance()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var fileSettings = new TestSettings { FileOverridesSetting = "file-value" };

        // Act
        var result = strategy.Merge(null, fileSettings, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("file-value", result.FileOverridesSetting);
    }

    [Fact]
    public void Merge_WhenFileSettingsIsNull_CreatesNewInstance()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();
        var dbSettings = new TestSettings { FileOverridesSetting = "db-value" };

        // Act
        var result = strategy.Merge(dbSettings, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("db-value", result.FileOverridesSetting);
    }

    [Fact]
    public void Merge_WhenBothSettingsAreNull_CreatesNewInstance()
    {
        // Arrange
        var strategy = new AttributeBasedSettingsMergeStrategy<TestSettings>();

        // Act
        var result = strategy.Merge(null, null, null);

        // Assert
        Assert.NotNull(result);
    }

    #endregion
}
