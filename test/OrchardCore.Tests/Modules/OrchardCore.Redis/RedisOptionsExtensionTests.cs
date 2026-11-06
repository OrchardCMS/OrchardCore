using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Redis;

namespace OrchardCore.Tests.Modules.OrchardCore.Redis;

public sealed class RedisOptionsExtensionTests
{
    private const string ValidConfiguration = "localhost:6379,abortConnect=false";

    [Theory]
    [InlineData("Redis")]
    [InlineData("OrchardCore_Redis")]
    public void BindConfiguration_ValidConfiguration_BindsOptions(string section)
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{section}:Configuration"] = ValidConfiguration,
            [$"{section}:InstancePrefix"] = "prefix:",
            [$"{section}:AllowAdmin"] = "true",
        });

        // Act
        var options = new RedisOptions().BindConfiguration(configuration, NullLogger.Instance);

        // Assert
        Assert.Equal(ValidConfiguration, options.Configuration);
        Assert.Equal("prefix:", options.InstancePrefix);
        Assert.True(options.ConfigurationOptions.AllowAdmin);
        Assert.Empty(options.Validate());
    }

    [Fact]
    public void BindConfiguration_SectionAndLegacySection_UsesValuesOfSection()
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            ["Redis:Configuration"] = ValidConfiguration,
            ["OrchardCore_Redis:Configuration"] = "legacy:6379",
            ["OrchardCore_Redis:InstancePrefix"] = "legacy:",
        });

        // Act
        var options = new RedisOptions().BindConfiguration(configuration, NullLogger.Instance);

        // Assert
        Assert.Equal(ValidConfiguration, options.Configuration);
        Assert.Equal("legacy:", options.InstancePrefix);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void BindConfiguration_MissingConfiguration_FailsValidationWithoutLogging(string value)
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            ["Redis:Configuration"] = value,
        });

        var logger = new Mock<ILogger<RedisOptionsExtensionTests>>();

        // Act
        var options = new RedisOptions().BindConfiguration(configuration, logger.Object);

        // Assert
        Assert.Null(options.ConfigurationOptions);
        Assert.Single(options.Validate());
        logger.Verify(l => l.Log(
            LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Never);
    }

    [Theory]
    [InlineData("Configuration", "unknownKeyword=value")]
    [InlineData("AllowAdmin", "malformed")]
    [InlineData("DisableCertificateVerification", "malformed")]
    public void BindConfiguration_InvalidValue_LogsErrorAndFailsValidation(string key, string value)
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            ["Redis:Configuration"] = ValidConfiguration,
            [$"Redis:{key}"] = value,
        });

        var logger = new Mock<ILogger<RedisOptionsExtensionTests>>();

        // Act
        var options = new RedisOptions().BindConfiguration(configuration, logger.Object);

        // Assert
        Assert.Null(options.ConfigurationOptions);
        Assert.Single(options.Validate());
        logger.Verify(l => l.Log(
            LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
            It.IsNotNull<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    private static ShellConfiguration BuildShellConfiguration(Dictionary<string, string> settings)
        => new(new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build());
}
