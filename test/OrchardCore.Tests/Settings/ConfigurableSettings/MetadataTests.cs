using OrchardCore.Settings;

namespace OrchardCore.Tests.Settings.ConfigurableSettings;

public class PropertyConfigurationMetadataTests
{
    [Fact]
    public void NewInstance_HasDefaultValues()
    {
        // Arrange & Act
        var metadata = new PropertyConfigurationMetadata();

        // Assert
        Assert.Null(metadata.PropertyName);
        Assert.Null(metadata.DisplayName);
        Assert.Equal(ConfigurationSource.Default, metadata.Source);
        Assert.Null(metadata.DatabaseValue);
        Assert.Null(metadata.FileValue);
        Assert.Null(metadata.EffectiveValue);
        Assert.Null(metadata.Attribute);
        Assert.Null(metadata.DefaultValue);
        Assert.False(metadata.IsSensitive);
        Assert.Null(metadata.SensitiveAttribute);
        Assert.Null(metadata.GroupName);
        Assert.Null(metadata.GroupAttribute);
        Assert.Null(metadata.PropertyType);
    }

    [Fact]
    public void CanSetAllProperties()
    {
        // Arrange & Act
        var attribute = new ConfigurationPropertyAttribute();
        var sensitiveAttribute = new SensitiveConfigurationAttribute();
        var groupAttribute = new ConfigurationGroupAttribute("Test Group");

        var metadata = new PropertyConfigurationMetadata
        {
            PropertyName = "TestProperty",
            DisplayName = "Test Property",
            Source = ConfigurationSource.ConfigurationFile,
            DatabaseValue = "db-value",
            FileValue = "file-value",
            EffectiveValue = "effective-value",
            Attribute = attribute,
            DefaultValue = "default",
            IsSensitive = true,
            SensitiveAttribute = sensitiveAttribute,
            GroupName = "Test Group",
            GroupAttribute = groupAttribute,
            PropertyType = typeof(string)
        };

        // Assert
        Assert.Equal("TestProperty", metadata.PropertyName);
        Assert.Equal("Test Property", metadata.DisplayName);
        Assert.Equal(ConfigurationSource.ConfigurationFile, metadata.Source);
        Assert.Equal("db-value", metadata.DatabaseValue);
        Assert.Equal("file-value", metadata.FileValue);
        Assert.Equal("effective-value", metadata.EffectiveValue);
        Assert.Same(attribute, metadata.Attribute);
        Assert.Equal("default", metadata.DefaultValue);
        Assert.True(metadata.IsSensitive);
        Assert.Same(sensitiveAttribute, metadata.SensitiveAttribute);
        Assert.Equal("Test Group", metadata.GroupName);
        Assert.Same(groupAttribute, metadata.GroupAttribute);
        Assert.Equal(typeof(string), metadata.PropertyType);
    }

    [Fact]
    public void MergeStrategy_WhenAttributeIsNull_ReturnsFileOverridesDatabase()
    {
        // Arrange
        var metadata = new PropertyConfigurationMetadata { Attribute = null };

        // Act & Assert
        Assert.Equal(PropertyMergeStrategy.FileOverridesDatabase, metadata.MergeStrategy);
    }

    [Fact]
    public void MergeStrategy_WhenAttributeIsSet_ReturnsAttributeStrategy()
    {
        // Arrange
        var attribute = new ConfigurationPropertyAttribute
        {
            MergeStrategy = PropertyMergeStrategy.DatabaseOverridesFile
        };
        var metadata = new PropertyConfigurationMetadata { Attribute = attribute };

        // Act & Assert
        Assert.Equal(PropertyMergeStrategy.DatabaseOverridesFile, metadata.MergeStrategy);
    }

    [Fact]
    public void IsOverriddenByFile_WhenSourceIsFile_ReturnsTrue()
    {
        // Arrange
        var metadata = new PropertyConfigurationMetadata
        {
            Source = ConfigurationSource.ConfigurationFile
        };

        // Assert
        Assert.True(metadata.IsOverriddenByFile);
    }

    [Fact]
    public void IsOverriddenByFile_WhenSourceIsDatabase_ReturnsFalse()
    {
        // Arrange
        var metadata = new PropertyConfigurationMetadata
        {
            Source = ConfigurationSource.Database
        };

        // Assert
        Assert.False(metadata.IsOverriddenByFile);
    }

    [Fact]
    public void CanConfigureViaUI_WhenAttributeIsNull_ReturnsTrue()
    {
        // Arrange
        var metadata = new PropertyConfigurationMetadata { Attribute = null };

        // Assert
        Assert.True(metadata.CanConfigureViaUI);
    }

    [Fact]
    public void CanConfigureViaUI_WhenAttributeDisallowsUI_ReturnsFalse()
    {
        // Arrange
        var metadata = new PropertyConfigurationMetadata
        {
            Attribute = new ConfigurationPropertyAttribute { AllowUIConfiguration = false }
        };

        // Assert
        Assert.False(metadata.CanConfigureViaUI);
    }

    [Fact]
    public void CanConfigureViaFile_WhenAttributeIsNull_ReturnsTrue()
    {
        // Arrange
        var metadata = new PropertyConfigurationMetadata { Attribute = null };

        // Assert
        Assert.True(metadata.CanConfigureViaFile);
    }

    [Fact]
    public void CanConfigureViaFile_WhenAttributeDisallowsFile_ReturnsFalse()
    {
        // Arrange
        var metadata = new PropertyConfigurationMetadata
        {
            Attribute = new ConfigurationPropertyAttribute { AllowFileConfiguration = false }
        };

        // Assert
        Assert.False(metadata.CanConfigureViaFile);
    }

    [Fact]
    public void GetMaskedValue_WhenNotSensitive_ReturnsOriginalValue()
    {
        // Arrange
        var metadata = new PropertyConfigurationMetadata
        {
            IsSensitive = false,
            EffectiveValue = "my-value"
        };

        // Act
        var result = metadata.GetMaskedValue();

        // Assert
        Assert.Equal("my-value", result);
    }

    [Fact]
    public void GetMaskedValue_WhenSensitive_MasksValue()
    {
        // Arrange
        var metadata = new PropertyConfigurationMetadata
        {
            IsSensitive = true,
            EffectiveValue = "my-secret-key",
            SensitiveAttribute = new SensitiveConfigurationAttribute { VisibleCharacters = 4 }
        };

        // Act
        var result = metadata.GetMaskedValue();

        // Assert
        // Should show last 4 characters unmasked
        Assert.EndsWith("-key", result);
        Assert.StartsWith("•", result);
    }

    [Fact]
    public void GetMaskedValue_WhenEffectiveValueIsNull_ReturnsNull()
    {
        // Arrange
        var metadata = new PropertyConfigurationMetadata
        {
            IsSensitive = true,
            EffectiveValue = null
        };

        // Act
        var result = metadata.GetMaskedValue();

        // Assert
        Assert.Null(result);
    }
}

public class SettingsConfigurationMetadataTests
{
    [Fact]
    public void NewInstance_HasDefaultValues()
    {
        // Arrange & Act
        var metadata = new SettingsConfigurationMetadata();

        // Assert
        Assert.Null(metadata.SettingsType);
        Assert.Null(metadata.ConfigurationKey);
        Assert.False(metadata.DisableUIConfiguration);
        Assert.False(metadata.IsConfiguredFromFile);
        Assert.NotNull(metadata.Properties);
        Assert.Empty(metadata.Properties);
    }

    [Fact]
    public void AddProperty_AddsPropertyToCollection()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();
        var property = new PropertyConfigurationMetadata
        {
            PropertyName = "Test"
        };

        // Act
        metadata.AddProperty(property);

        // Assert
        Assert.Single(metadata.Properties);
        Assert.True(metadata.Properties.ContainsKey("Test"));
    }

    [Fact]
    public void IsPropertyOverridden_WhenSourceIsFile_ReturnsTrue()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();
        metadata.AddProperty(new PropertyConfigurationMetadata
        {
            PropertyName = "Test",
            Source = ConfigurationSource.ConfigurationFile
        });

        // Act
        var result = metadata.IsPropertyOverridden("Test");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsPropertyOverridden_WhenSourceIsDatabase_ReturnsFalse()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();
        metadata.AddProperty(new PropertyConfigurationMetadata
        {
            PropertyName = "Test",
            Source = ConfigurationSource.Database
        });

        // Act
        var result = metadata.IsPropertyOverridden("Test");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsPropertyOverridden_WhenPropertyNotFound_ReturnsFalse()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();

        // Act
        var result = metadata.IsPropertyOverridden("NonExistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetPropertySource_WhenPropertyExists_ReturnsSource()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();
        metadata.AddProperty(new PropertyConfigurationMetadata
        {
            PropertyName = "Test",
            Source = ConfigurationSource.Database
        });

        // Act
        var result = metadata.GetPropertySource("Test");

        // Assert
        Assert.Equal(ConfigurationSource.Database, result);
    }

    [Fact]
    public void GetPropertySource_WhenPropertyNotFound_ReturnsDefault()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();

        // Act
        var result = metadata.GetPropertySource("NonExistent");

        // Assert
        Assert.Equal(ConfigurationSource.Default, result);
    }

    [Fact]
    public void GetEffectiveValue_WhenPropertyExists_ReturnsValue()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();
        metadata.AddProperty(new PropertyConfigurationMetadata
        {
            PropertyName = "Test",
            EffectiveValue = "test-value"
        });

        // Act
        var result = metadata.GetEffectiveValue("Test");

        // Assert
        Assert.Equal("test-value", result);
    }

    [Fact]
    public void GetEffectiveValue_WhenPropertyNotFound_ReturnsNull()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();

        // Act
        var result = metadata.GetEffectiveValue("NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetPropertyMetadata_WhenPropertyExists_ReturnsMetadata()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();
        var property = new PropertyConfigurationMetadata { PropertyName = "Test" };
        metadata.AddProperty(property);

        // Act
        var result = metadata.GetPropertyMetadata("Test");

        // Assert
        Assert.Same(property, result);
    }

    [Fact]
    public void GetPropertyMetadata_WhenPropertyNotFound_ReturnsNull()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();

        // Act
        var result = metadata.GetPropertyMetadata("NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetPropertyMetadata_IsCaseInsensitive()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();
        var property = new PropertyConfigurationMetadata { PropertyName = "TestProperty" };
        metadata.AddProperty(property);

        // Act
        var result = metadata.GetPropertyMetadata("testproperty");

        // Assert
        Assert.Same(property, result);
    }

    [Fact]
    public void GetOverriddenProperties_ReturnsOnlyOverriddenProperties()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();
        var overridden = new PropertyConfigurationMetadata
        {
            PropertyName = "Overridden",
            Source = ConfigurationSource.ConfigurationFile
        };
        var notOverridden = new PropertyConfigurationMetadata
        {
            PropertyName = "NotOverridden",
            Source = ConfigurationSource.Database
        };
        metadata.AddProperty(overridden);
        metadata.AddProperty(notOverridden);

        // Act
        var result = metadata.GetOverriddenProperties().ToList();

        // Assert
        Assert.Single(result);
        Assert.Same(overridden, result[0]);
    }

    [Fact]
    public void GetPropertiesByGroup_GroupsPropertiesCorrectly()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();
        metadata.AddProperty(new PropertyConfigurationMetadata
        {
            PropertyName = "P1",
            GroupName = "Group1"
        });
        metadata.AddProperty(new PropertyConfigurationMetadata
        {
            PropertyName = "P2",
            GroupName = "Group1"
        });
        metadata.AddProperty(new PropertyConfigurationMetadata
        {
            PropertyName = "P3",
            GroupName = "Group2"
        });
        metadata.AddProperty(new PropertyConfigurationMetadata
        {
            PropertyName = "P4",
            GroupName = null
        });

        // Act
        var groups = metadata.GetPropertiesByGroup().ToList();

        // Assert
        Assert.Equal(3, groups.Count);
    }

    [Fact]
    public void GetUIConfigurableProperties_WhenUIDisabled_ReturnsEmpty()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata
        {
            DisableUIConfiguration = true
        };
        metadata.AddProperty(new PropertyConfigurationMetadata
        {
            PropertyName = "Test",
            Attribute = new ConfigurationPropertyAttribute { AllowUIConfiguration = true }
        });

        // Act
        var result = metadata.GetUIConfigurableProperties().ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetUIConfigurableProperties_ReturnsConfigurableProperties()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata
        {
            DisableUIConfiguration = false
        };
        var configurable = new PropertyConfigurationMetadata
        {
            PropertyName = "Configurable",
            Attribute = new ConfigurationPropertyAttribute { AllowUIConfiguration = true }
        };
        var notConfigurable = new PropertyConfigurationMetadata
        {
            PropertyName = "NotConfigurable",
            Attribute = new ConfigurationPropertyAttribute { AllowUIConfiguration = false }
        };
        metadata.AddProperty(configurable);
        metadata.AddProperty(notConfigurable);

        // Act
        var result = metadata.GetUIConfigurableProperties().ToList();

        // Assert
        Assert.Single(result);
        Assert.Same(configurable, result[0]);
    }

    [Fact]
    public void GetSensitiveProperties_ReturnsOnlySensitiveProperties()
    {
        // Arrange
        var metadata = new SettingsConfigurationMetadata();
        var sensitive = new PropertyConfigurationMetadata
        {
            PropertyName = "Sensitive",
            IsSensitive = true
        };
        var notSensitive = new PropertyConfigurationMetadata
        {
            PropertyName = "NotSensitive",
            IsSensitive = false
        };
        metadata.AddProperty(sensitive);
        metadata.AddProperty(notSensitive);

        // Act
        var result = metadata.GetSensitiveProperties().ToList();

        // Assert
        Assert.Single(result);
        Assert.Same(sensitive, result[0]);
    }
}
