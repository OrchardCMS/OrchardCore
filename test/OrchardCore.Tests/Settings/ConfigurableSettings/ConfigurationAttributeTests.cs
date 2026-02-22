using OrchardCore.Settings;

namespace OrchardCore.Tests.Settings.ConfigurableSettings;

public class ConfigurationPropertyAttributeTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var attribute = new ConfigurationPropertyAttribute();

        // Assert
        Assert.Equal(PropertyMergeStrategy.FileOverridesDatabase, attribute.MergeStrategy);
        Assert.True(attribute.AllowFileConfiguration);
        Assert.True(attribute.AllowUIConfiguration);
        Assert.Null(attribute.DisplayName);
        Assert.Null(attribute.CustomMergeFunction);
        Assert.Equal(0, attribute.Priority);
        Assert.Null(attribute.Description);
    }

    [Fact]
    public void CanSetMergeStrategy()
    {
        // Arrange & Act
        var attribute = new ConfigurationPropertyAttribute
        {
            MergeStrategy = PropertyMergeStrategy.DatabaseOverridesFile
        };

        // Assert
        Assert.Equal(PropertyMergeStrategy.DatabaseOverridesFile, attribute.MergeStrategy);
    }

    [Fact]
    public void CanSetAllProperties()
    {
        // Arrange & Act
        var attribute = new ConfigurationPropertyAttribute
        {
            MergeStrategy = PropertyMergeStrategy.Custom,
            AllowFileConfiguration = false,
            AllowUIConfiguration = false,
            DisplayName = "Test Display Name",
            CustomMergeFunction = typeof(TestMergeFunction),
            Priority = 10,
            Description = "Test Description"
        };

        // Assert
        Assert.Equal(PropertyMergeStrategy.Custom, attribute.MergeStrategy);
        Assert.False(attribute.AllowFileConfiguration);
        Assert.False(attribute.AllowUIConfiguration);
        Assert.Equal("Test Display Name", attribute.DisplayName);
        Assert.Equal(typeof(TestMergeFunction), attribute.CustomMergeFunction);
        Assert.Equal(10, attribute.Priority);
        Assert.Equal("Test Description", attribute.Description);
    }

    private class TestMergeFunction : IPropertyMergeFunction
    {
        public object Merge(object databaseValue, object fileValue, PropertyMergeContext context)
        {
            return fileValue ?? databaseValue;
        }
    }
}

public class DefaultConfigurationValueAttributeTests
{
    [Fact]
    public void CanSetStringValue()
    {
        // Arrange & Act
        var attribute = new DefaultConfigurationValueAttribute("test-value");

        // Assert
        Assert.Equal("test-value", attribute.Value);
    }

    [Fact]
    public void CanSetIntValue()
    {
        // Arrange & Act
        var attribute = new DefaultConfigurationValueAttribute(42);

        // Assert
        Assert.Equal(42, attribute.Value);
    }

    [Fact]
    public void CanSetBoolValue()
    {
        // Arrange & Act
        var attribute = new DefaultConfigurationValueAttribute(true);

        // Assert
        Assert.Equal(true, attribute.Value);
    }

    [Fact]
    public void CanSetDoubleValue()
    {
        // Arrange & Act
        var attribute = new DefaultConfigurationValueAttribute(3.14);

        // Assert
        Assert.Equal(3.14, attribute.Value);
    }
}

public class SensitiveConfigurationAttributeTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var attribute = new SensitiveConfigurationAttribute();

        // Assert
        Assert.Equal('•', attribute.MaskCharacter);
        Assert.Equal(4, attribute.VisibleCharacters);
        Assert.False(attribute.AllowReveal);
    }

    [Fact]
    public void CanSetAllProperties()
    {
        // Arrange & Act
        var attribute = new SensitiveConfigurationAttribute
        {
            MaskCharacter = '*',
            VisibleCharacters = 2,
            AllowReveal = true
        };

        // Assert
        Assert.Equal('*', attribute.MaskCharacter);
        Assert.Equal(2, attribute.VisibleCharacters);
        Assert.True(attribute.AllowReveal);
    }
}

public class ConfigurationGroupAttributeTests
{
    [Fact]
    public void CanSetGroupName()
    {
        // Arrange & Act
        var attribute = new ConfigurationGroupAttribute("Advanced Settings");

        // Assert
        Assert.Equal("Advanced Settings", attribute.GroupName);
    }

    [Fact]
    public void DefaultOrder_IsZero()
    {
        // Arrange & Act
        var attribute = new ConfigurationGroupAttribute("Test");

        // Assert
        Assert.Equal(0, attribute.Order);
    }

    [Fact]
    public void CanSetOrder()
    {
        // Arrange & Act
        var attribute = new ConfigurationGroupAttribute("Test")
        {
            Order = 5
        };

        // Assert
        Assert.Equal(5, attribute.Order);
    }

    [Fact]
    public void CanSetDisplayName()
    {
        // Arrange & Act
        var attribute = new ConfigurationGroupAttribute("test-group")
        {
            DisplayName = "Test Group Display"
        };

        // Assert
        Assert.Equal("Test Group Display", attribute.DisplayName);
    }

    [Fact]
    public void CanSetDescription()
    {
        // Arrange & Act
        var attribute = new ConfigurationGroupAttribute("test-group")
        {
            Description = "Test group description"
        };

        // Assert
        Assert.Equal("Test group description", attribute.Description);
    }

    [Fact]
    public void Constructor_ThrowsOnNullGroupName()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConfigurationGroupAttribute(null));
    }

    [Fact]
    public void Constructor_ThrowsOnEmptyGroupName()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new ConfigurationGroupAttribute(""));
    }
}
