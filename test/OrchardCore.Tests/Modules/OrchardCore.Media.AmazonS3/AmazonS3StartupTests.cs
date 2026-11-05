using Amazon.S3;
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
    public void TryBindConfiguration_MalformedOption_ReturnsFalseAndFailsValidation(string option)
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = "us-east-1",
            [$"{Section}:BucketName"] = "media",
            [$"{Section}:{option}"] = "malformed",
        });

        var options = new AwsStorageOptions();

        // Act
        var result = options.TryBindConfiguration(Section, LegacySection, configuration, NullLogger.Instance);

        // Assert
        Assert.False(result);
        Assert.Null(options.AwsOptions);
        Assert.Single(options.Validate());
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

        var options = new AwsStorageOptions();

        // Act
        var result = options.TryBindConfiguration($"{Section}:ImageCache", $"{LegacySection}_ImageSharp_Cache", configuration, NullLogger.Instance);

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
        var options = new AwsStorageOptions();

        // Act
        var result = options.TryBindConfiguration(Section, LegacySection, configuration, NullLogger.Instance);

        // Assert
        Assert.True(result);
        var error = Assert.Single(options.Validate());
        Assert.Contains("BucketName", error.ErrorMessage);
    }

    private static ShellConfiguration BuildShellConfiguration(Dictionary<string, string> settings)
        => new(new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build());
}
