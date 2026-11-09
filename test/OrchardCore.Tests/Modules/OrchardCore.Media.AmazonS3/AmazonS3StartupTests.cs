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
    public void BindConfiguration_MalformedOption_FailsValidation()
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>
        {
            [$"{Section}:Region"] = "us-east-1",
            [$"{Section}:BucketName"] = "media",
            [$"{Section}:Credentials:AccessKey"] = "malformed",
        });

        // Act
        var options = new AwsStorageOptions().BindConfiguration(Section, LegacySection, configuration, NullLogger.Instance);

        // Assert
        Assert.Null(options.AwsOptions);
        Assert.Single(options.Validate());
    }

    [Fact]
    public void BindConfiguration_ImageCacheWithoutCredentials_UsesCredentialsOfParentSection()
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
        var options = new AwsStorageOptions().BindConfiguration($"{Section}:ImageCache", $"{LegacySection}_ImageSharp_Cache", configuration, NullLogger.Instance);

        // Assert
        Assert.NotNull(options.AwsOptions.Credentials);
        Assert.Empty(options.Validate());
    }

    [Fact]
    public void Validate_WithoutConfiguration_OnlyReportsBucketName()
    {
        // Arrange
        var configuration = BuildShellConfiguration(new Dictionary<string, string>());

        // Act
        var options = new AwsStorageOptions().BindConfiguration(Section, LegacySection, configuration, NullLogger.Instance);

        // Assert
        var result = Assert.Single(options.Validate());
        Assert.Contains("BucketName", result.ErrorMessage);
    }

    private static ShellConfiguration BuildShellConfiguration(Dictionary<string, string> settings)
        => new(new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build());
}
