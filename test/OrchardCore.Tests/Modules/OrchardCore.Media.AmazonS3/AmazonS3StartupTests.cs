using Amazon.S3;
using Fluid;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Media;
using OrchardCore.Media.AmazonS3;
using OrchardCore.Media.AmazonS3.Services;
using OrchardCore.Media.Services;
using AmazonS3Startup = OrchardCore.Media.AmazonS3.Startup;

namespace OrchardCore.Tests.Modules.OrchardCore.Media.AmazonS3;

public sealed class AmazonS3StartupTests
{
    private const string Section = "Media:AmazonS3";
    private const string LegacySection = "OrchardCore_Media_AmazonS3";

    [Fact]
    public void ConfigureServices_ImageCacheWithoutMediaStore_RegistersClient()
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = "us-east-1",
            [$"{Section}:ImageCache:BucketName"] = "image-cache",
        });

        var services = new ServiceCollection();

        // Act
        new AmazonS3Startup(configuration, NullLogger<AmazonS3Startup>.Instance).ConfigureServices(services);
        new MediaAmazonS3ImageCacheStartup(configuration, NullLogger<MediaAmazonS3ImageCacheStartup>.Instance).ConfigureServices(services);

        // Assert
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IAmazonS3));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IResizedImageCache));
    }

    [Fact]
    public void ConfigureServices_ImageCacheWithMediaStore_KeepsClientOfMediaStore()
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = "us-east-1",
            [$"{Section}:BucketName"] = "media",
            [$"{Section}:ImageCache:BucketName"] = "image-cache",
        });

        var services = new ServiceCollection();

        // Act
        new AmazonS3Startup(configuration, NullLogger<AmazonS3Startup>.Instance).ConfigureServices(services);
        new MediaAmazonS3ImageCacheStartup(configuration, NullLogger<MediaAmazonS3ImageCacheStartup>.Instance).ConfigureServices(services);

        // Assert
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IAmazonS3));
    }

    [Fact]
    public void ConfigureServices_WithoutImageCache_DoesNotRegisterClient()
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>());
        var services = new ServiceCollection();

        // Act
        new AmazonS3Startup(configuration, NullLogger<AmazonS3Startup>.Instance).ConfigureServices(services);
        new MediaAmazonS3ImageCacheStartup(configuration, NullLogger<MediaAmazonS3ImageCacheStartup>.Instance).ConfigureServices(services);

        // Assert
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IAmazonS3));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IResizedImageCache));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("us-east-1", null)]
    [InlineData(null, "media")]
    public void ConfigureServices_MediaStoreNotActive_KeepsDefaultTusStore(string region, string bucketName)
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = region,
            [$"{Section}:BucketName"] = bucketName,
        });

        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ITusTempStore>());

        // Act
        new MediaAmazonS3TusStartup(configuration, NullLogger<MediaAmazonS3TusStartup>.Instance).ConfigureServices(services);

        // Assert
        Assert.DoesNotContain(services, descriptor => descriptor.ImplementationType == typeof(S3TusTempStore));
    }

    [Theory]
    [InlineData(Section)]
    [InlineData(LegacySection)]
    public void ConfigureServices_MediaStoreActive_ReplacesTusStore(string section)
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{section}:Region"] = "us-east-1",
            [$"{section}:BucketName"] = "media",
        });

        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ITusTempStore>());

        // Act
        new MediaAmazonS3TusStartup(configuration, NullLogger<MediaAmazonS3TusStartup>.Instance).ConfigureServices(services);

        // Assert
        var descriptor = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(ITusTempStore));
        Assert.Equal(typeof(S3TusTempStore), descriptor.ImplementationType);
    }

    [Theory]
    [InlineData("Timeout")]
    [InlineData("CreateBucket")]
    [InlineData("Credentials:AccessKey")]
    public void ConfigureServices_MalformedOption_DoesNotThrowAndDoesNotRegisterClient(string option)
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = "us-east-1",
            [$"{Section}:BucketName"] = "media",
            [$"{Section}:{option}"] = "malformed",
        });

        var services = new ServiceCollection();

        // Act
        new AmazonS3Startup(configuration, NullLogger<AmazonS3Startup>.Instance).ConfigureServices(services);

        // Assert
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IAmazonS3));
    }

    [Fact]
    public void ConfigureServices_ImageCacheMalformedOption_DoesNotRegisterClient()
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = "us-east-1",
            [$"{Section}:Timeout"] = "malformed",
            [$"{Section}:ImageCache:BucketName"] = "image-cache",
        });

        var services = new ServiceCollection();

        // Act
        new MediaAmazonS3ImageCacheStartup(configuration, NullLogger<MediaAmazonS3ImageCacheStartup>.Instance).ConfigureServices(services);

        // Assert
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IAmazonS3));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IResizedImageCache));
    }

    [Fact]
    public void ConfigureServices_MalformedOption_KeepsDefaultTusStore()
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = "us-east-1",
            [$"{Section}:BucketName"] = "media",
            [$"{Section}:Timeout"] = "malformed",
        });

        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ITusTempStore>());

        // Act
        new MediaAmazonS3TusStartup(configuration, NullLogger<MediaAmazonS3TusStartup>.Instance).ConfigureServices(services);

        // Assert
        Assert.DoesNotContain(services, descriptor => descriptor.ImplementationType == typeof(S3TusTempStore));
    }

    [Theory]
    [InlineData("Timeout")]
    [InlineData("CreateBucket")]
    [InlineData("Credentials:AccessKey")]
    public void TryBindConfiguration_MalformedOption_ReturnsFalseAndLogsError(string option)
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = "us-east-1",
            [$"{Section}:BucketName"] = "media",
            [$"{Section}:{option}"] = "malformed",
        });

        var logger = new ListLogger();

        // Act
        var result = configuration.TryBindConfiguration<AwsStorageOptions>(Section, LegacySection, logger, out var options);

        // Assert
        Assert.False(result);
        Assert.Null(options);
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Critical, entry.Level);
        Assert.NotNull(entry.Exception);
    }

    [Fact]
    public void TryBindConfiguration_ImageCacheWithoutCredentials_UsesCredentialsOfParentSection()
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = "us-east-1",
            [$"{Section}:Credentials:AccessKey"] = "access-key",
            [$"{Section}:Credentials:SecretKey"] = "secret-key",
            [$"{Section}:ImageCache:BucketName"] = "image-cache",
        });

        // Act
        var result = configuration.TryBindConfiguration<AwsMediaImageCacheOptions>($"{Section}:ImageCache", $"{LegacySection}_ImageSharp_Cache", NullLogger.Instance, out var options);

        // Assert
        Assert.True(result);
        Assert.NotNull(options.AwsOptions.Credentials);
        Assert.Empty(options.Validate());
    }

    [Fact]
    public void TryBindConfiguration_WithoutConfiguration_OnlyReportsBucketName()
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>());

        // Act
        var result = configuration.TryBindConfiguration<AwsStorageOptions>(Section, LegacySection, NullLogger.Instance, out var options);

        // Assert
        Assert.True(result);
        var error = Assert.Single(options.Validate());
        Assert.Contains("BucketName", error.ErrorMessage);
    }

    [Fact]
    public void Configure_ValidConfiguration_SetsOptions()
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = "us-east-1",
            [$"{Section}:BucketName"] = "{{ ShellSettings.Name }}-media",
            [$"{Section}:BasePath"] = "{{ ShellSettings.Name }}/Media",
            [$"{Section}:CreateBucket"] = "true",
            [$"{Section}:RemoveBucket"] = "true",
        });

        var optionsConfiguration = new AwsStorageOptionsConfiguration(
            new FluidParser(),
            configuration,
            new ShellSettings { Name = "tenant1" },
            NullLogger<AwsStorageOptionsConfiguration>.Instance);

        var options = new AwsStorageOptions();

        // Act
        optionsConfiguration.Configure(options);

        // Assert
        Assert.Equal("tenant1-media", options.BucketName);
        Assert.Equal("tenant1/Media", options.BasePath);
        Assert.True(options.CreateBucket);
        Assert.True(options.RemoveBucket);
        Assert.Equal("us-east-1", options.AwsOptions.Region.SystemName);
        Assert.Empty(options.Validate());
    }

    [Fact]
    public void Configure_ImageCacheValidConfiguration_SetsOptions()
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = "us-east-1",
            [$"{Section}:ImageCache:BucketName"] = "{{ ShellSettings.Name }}-image-cache",
            [$"{Section}:ImageCache:BasePath"] = "{{ ShellSettings.Name }}/ImageCache",
            [$"{Section}:ImageCache:CreateBucket"] = "true",
            [$"{Section}:ImageCache:RemoveBucket"] = "true",
        });

        var optionsConfiguration = new AwsMediaImageCacheOptionsConfiguration(
            new FluidParser(),
            configuration,
            new ShellSettings { Name = "tenant1" },
            NullLogger<AwsStorageOptionsConfiguration>.Instance);

        var options = new AwsMediaImageCacheOptions();

        // Act
        optionsConfiguration.Configure(options);

        // Assert
        Assert.Equal("tenant1-image-cache", options.BucketName);
        Assert.Equal("tenant1/ImageCache", options.BasePath);
        Assert.True(options.CreateBucket);
        Assert.True(options.RemoveBucket);
        Assert.Equal("us-east-1", options.AwsOptions.Region.SystemName);
        Assert.Empty(options.Validate());
    }

    [Theory]
    [InlineData("Timeout")]
    [InlineData("CreateBucket")]
    [InlineData("Credentials:AccessKey")]
    public void Configure_MalformedOption_FailsValidation(string option)
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = "us-east-1",
            [$"{Section}:BucketName"] = "media",
            [$"{Section}:{option}"] = "malformed",
        });

        var optionsConfiguration = new AwsStorageOptionsConfiguration(
            new FluidParser(),
            configuration,
            new ShellSettings { Name = "tenant1" },
            NullLogger<AwsStorageOptionsConfiguration>.Instance);

        var options = new AwsStorageOptions();

        // Act
        optionsConfiguration.Configure(options);

        // Assert
        Assert.Null(options.BucketName);
        Assert.Null(options.AwsOptions);
        var error = Assert.Single(options.Validate());
        Assert.Equal(AmazonS3Constants.ValidationMessages.BucketNameIsEmpty, error.ErrorMessage);
    }

    [Theory]
    [InlineData("Timeout")]
    [InlineData("ImageCache:CreateBucket")]
    public void Configure_ImageCacheMalformedOption_FailsValidation(string option)
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = "us-east-1",
            [$"{Section}:ImageCache:BucketName"] = "image-cache",
            [$"{Section}:{option}"] = "malformed",
        });

        var optionsConfiguration = new AwsMediaImageCacheOptionsConfiguration(
            new FluidParser(),
            configuration,
            new ShellSettings { Name = "tenant1" },
            NullLogger<AwsStorageOptionsConfiguration>.Instance);

        var options = new AwsMediaImageCacheOptions();

        // Act
        optionsConfiguration.Configure(options);

        // Assert
        Assert.Null(options.BucketName);
        Assert.Null(options.AwsOptions);
        var error = Assert.Single(options.Validate());
        Assert.Equal(AmazonS3Constants.ValidationMessages.BucketNameIsEmpty, error.ErrorMessage);
    }

    private static ShellConfiguration BuildShellConfiguration(Dictionary<string, string> settings)
        => new(new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build());

    private sealed class ListLogger : ILogger
    {
        public List<(LogLevel Level, Exception Exception)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => Entries.Add((logLevel, exception));
    }
}
