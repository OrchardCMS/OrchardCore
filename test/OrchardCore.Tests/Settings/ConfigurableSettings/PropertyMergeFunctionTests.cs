using OrchardCore.Settings;

namespace OrchardCore.Tests.Settings.ConfigurableSettings;

public class PropertyMergeFunctionTests
{
    #region Test Merge Function

    public class AddingMergeFunction : IPropertyMergeFunction
    {
        public object Merge(object databaseValue, object fileValue, PropertyMergeContext context)
        {
            var dbInt = databaseValue as int? ?? 0;
            var fileInt = fileValue as int? ?? 0;
            return dbInt + fileInt;
        }
    }

    public class ConcatMergeFunction : IPropertyMergeFunction
    {
        public object Merge(object databaseValue, object fileValue, PropertyMergeContext context)
        {
            var dbStr = databaseValue as string ?? "";
            var fileStr = fileValue as string ?? "";
            return $"{dbStr}{fileStr}";
        }
    }

    public class ContextAwareMergeFunction : IPropertyMergeFunction
    {
        public object Merge(object databaseValue, object fileValue, PropertyMergeContext context)
        {
            if (context.DisableUIConfiguration)
            {
                return fileValue;
            }

            return databaseValue ?? fileValue;
        }
    }

    #endregion

    [Fact]
    public void AddingMergeFunction_AddsBothValues()
    {
        // Arrange
        var mergeFunction = new AddingMergeFunction();
        var context = new PropertyMergeContext
        {
            PropertyName = "Test",
            PropertyType = typeof(int),
            SettingsType = typeof(object)
        };

        // Act
        var result = mergeFunction.Merge(5, 10, context);

        // Assert
        Assert.Equal(15, result);
    }

    [Fact]
    public void ConcatMergeFunction_ConcatenatesStrings()
    {
        // Arrange
        var mergeFunction = new ConcatMergeFunction();
        var context = new PropertyMergeContext
        {
            PropertyName = "Test",
            PropertyType = typeof(string),
            SettingsType = typeof(object)
        };

        // Act
        var result = mergeFunction.Merge("Hello", " World", context);

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void ContextAwareMergeFunction_WhenUIDisabled_ReturnsFileValue()
    {
        // Arrange
        var mergeFunction = new ContextAwareMergeFunction();
        var context = new PropertyMergeContext
        {
            PropertyName = "Test",
            PropertyType = typeof(string),
            SettingsType = typeof(object),
            DisableUIConfiguration = true
        };

        // Act
        var result = mergeFunction.Merge("db-value", "file-value", context);

        // Assert
        Assert.Equal("file-value", result);
    }

    [Fact]
    public void ContextAwareMergeFunction_WhenUIEnabled_ReturnsDatabaseValue()
    {
        // Arrange
        var mergeFunction = new ContextAwareMergeFunction();
        var context = new PropertyMergeContext
        {
            PropertyName = "Test",
            PropertyType = typeof(string),
            SettingsType = typeof(object),
            DisableUIConfiguration = false
        };

        // Act
        var result = mergeFunction.Merge("db-value", "file-value", context);

        // Assert
        Assert.Equal("db-value", result);
    }
}

public class PropertyMergeContextTests
{
    [Fact]
    public void NewContext_HasDefaultValues()
    {
        // Arrange & Act
        var context = new PropertyMergeContext();

        // Assert
        Assert.Null(context.PropertyName);
        Assert.Null(context.PropertyType);
        Assert.Null(context.SettingsType);
        Assert.Null(context.Attribute);
        Assert.False(context.DisableUIConfiguration);
        Assert.Null(context.ServiceProvider);
    }

    [Fact]
    public void CanSetAllProperties()
    {
        // Arrange
        var attribute = new ConfigurationPropertyAttribute();

        // Act
        var context = new PropertyMergeContext
        {
            PropertyName = "TestProperty",
            PropertyType = typeof(string),
            SettingsType = typeof(TestSettings),
            Attribute = attribute,
            DisableUIConfiguration = true,
            ServiceProvider = null
        };

        // Assert
        Assert.Equal("TestProperty", context.PropertyName);
        Assert.Equal(typeof(string), context.PropertyType);
        Assert.Equal(typeof(TestSettings), context.SettingsType);
        Assert.Same(attribute, context.Attribute);
        Assert.True(context.DisableUIConfiguration);
    }

    private class TestSettings : IConfigurableSettings
    {
        public bool DisableUIConfiguration { get; set; }
    }
}
