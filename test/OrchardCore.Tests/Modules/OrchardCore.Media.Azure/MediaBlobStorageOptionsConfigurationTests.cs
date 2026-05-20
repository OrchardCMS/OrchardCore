using System.Collections.Concurrent;
using Fluid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Media.Azure;
using OrchardCore.Media.Azure.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Media.Azure;

public class MediaBlobStorageOptionsConfigurationTests
{
    private const string ValidConnectionString = "DefaultEndpointsProtocol=https;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";

    [Fact]
    public void ParsesLiquidTemplate_AndLowercasesContainerName()
    {
        var (config, logger) = CreateConfiguration(new Dictionary<string, string?>
        {
            ["OrchardCore_Media_Azure:ConnectionString"] = ValidConnectionString,
            ["OrchardCore_Media_Azure:ContainerName"] = "{{ ShellSettings.Name }}-media",
        }, shellName: "Tenant1");

        var options = new MediaBlobStorageOptions();
        config.Configure(options);

        Assert.Equal("tenant1-media", options.ContainerName);
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Critical);
    }

    [Theory]
    [InlineData("default_imagecache")]    // underscore (the canonical #17597 case)
    [InlineData("media.cache")]           // illegal character (period)
    [InlineData("ab")]                     // too short (< 3)
    [InlineData("-leading")]               // starts with hyphen
    [InlineData("trailing-")]              // ends with hyphen
    [InlineData("double--hyphen")]         // consecutive hyphens
    public void InvalidResolvedName_LogsCritical_DoesNotThrow(string rawName)
    {
        var (config, logger) = CreateConfiguration(new Dictionary<string, string?>
        {
            ["OrchardCore_Media_Azure:ConnectionString"] = ValidConnectionString,
            ["OrchardCore_Media_Azure:ContainerName"] = rawName,
        });

        var options = new MediaBlobStorageOptions();
        config.Configure(options);

        Assert.Contains(logger.Entries, e =>
            e.Level == LogLevel.Critical &&
            e.Message.Contains("not a valid Azure Blob container name", StringComparison.Ordinal));
    }

    [Fact]
    public void TooLongName_LogsCritical_DoesNotThrow()
    {
        var rawName = new string('a', 64);
        var (config, logger) = CreateConfiguration(new Dictionary<string, string?>
        {
            ["OrchardCore_Media_Azure:ConnectionString"] = ValidConnectionString,
            ["OrchardCore_Media_Azure:ContainerName"] = rawName,
        });

        var options = new MediaBlobStorageOptions();
        config.Configure(options);

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Critical);
    }

    [Fact]
    public void CreateContainer_RoundTripsFromConfig_WhenFalse()
    {
        var (config, _) = CreateConfiguration(new Dictionary<string, string?>
        {
            ["OrchardCore_Media_Azure:ConnectionString"] = ValidConnectionString,
            ["OrchardCore_Media_Azure:ContainerName"] = "media",
            ["OrchardCore_Media_Azure:CreateContainer"] = "false",
        });

        var options = new MediaBlobStorageOptions();
        config.Configure(options);

        Assert.False(options.CreateContainer);
    }

    [Fact]
    public void RemoveContainer_AndRemoveFilesFromBasePath_RoundTripFromConfig()
    {
        var (config, _) = CreateConfiguration(new Dictionary<string, string?>
        {
            ["OrchardCore_Media_Azure:ConnectionString"] = ValidConnectionString,
            ["OrchardCore_Media_Azure:ContainerName"] = "media",
            ["OrchardCore_Media_Azure:RemoveContainer"] = "true",
            ["OrchardCore_Media_Azure:RemoveFilesFromBasePath"] = "true",
        });

        var options = new MediaBlobStorageOptions();
        config.Configure(options);

        Assert.True(options.RemoveContainer);
        Assert.True(options.RemoveFilesFromBasePath);
    }

    private static (MediaBlobStorageOptionsConfiguration Config, ListLogger<MediaBlobStorageOptionsConfiguration> Logger) CreateConfiguration(
        IDictionary<string, string?> data,
        string shellName = "Default")
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();

        var shellConfiguration = new ShellConfiguration(configuration);
        var shellSettings = new ShellSettings { Name = shellName };
        var logger = new ListLogger<MediaBlobStorageOptionsConfiguration>();

        var config = new MediaBlobStorageOptionsConfiguration(
            new FluidParser(),
            shellConfiguration,
            shellSettings,
            logger);

        return (config, logger);
    }

    private sealed class ListLogger<T> : ILogger<T>
    {
        public ConcurrentBag<(LogLevel Level, string Message, Exception Exception)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception), exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
