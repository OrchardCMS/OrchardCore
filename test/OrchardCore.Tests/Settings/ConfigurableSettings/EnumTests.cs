using OrchardCore.Settings;

namespace OrchardCore.Tests.Settings.ConfigurableSettings;

public class PropertyMergeStrategyTests
{
    [Fact]
    public void AllMergeStrategiesAreDefined()
    {
        // Verify all expected strategies exist
        var strategies = Enum.GetValues<PropertyMergeStrategy>();

        Assert.Contains(PropertyMergeStrategy.FileOverridesDatabase, strategies);
        Assert.Contains(PropertyMergeStrategy.DatabaseOverridesFile, strategies);
        Assert.Contains(PropertyMergeStrategy.FileAsDefault, strategies);
        Assert.Contains(PropertyMergeStrategy.DatabaseAsDefault, strategies);
        Assert.Contains(PropertyMergeStrategy.Merge, strategies);
        Assert.Contains(PropertyMergeStrategy.FileOnly, strategies);
        Assert.Contains(PropertyMergeStrategy.DatabaseOnly, strategies);
        Assert.Contains(PropertyMergeStrategy.Custom, strategies);
    }

    [Fact]
    public void StrategyEnumValues_AreUnique()
    {
        // Arrange
        var strategies = Enum.GetValues<PropertyMergeStrategy>();
        var values = strategies.Cast<int>().ToList();

        // Assert
        Assert.Equal(values.Count, values.Distinct().Count());
    }
}

public class ConfigurationSourceTests
{
    [Fact]
    public void AllConfigurationSourcesAreDefined()
    {
        // Verify all expected sources exist
        var sources = Enum.GetValues<ConfigurationSource>();

        Assert.Contains(ConfigurationSource.Default, sources);
        Assert.Contains(ConfigurationSource.Database, sources);
        Assert.Contains(ConfigurationSource.ConfigurationFile, sources);
    }

    [Fact]
    public void SourceEnumValues_AreUnique()
    {
        // Arrange
        var sources = Enum.GetValues<ConfigurationSource>();
        var values = sources.Cast<int>().ToList();

        // Assert
        Assert.Equal(values.Count, values.Distinct().Count());
    }
}
