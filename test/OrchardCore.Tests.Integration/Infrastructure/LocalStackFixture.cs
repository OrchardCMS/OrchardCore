using Amazon.Runtime;
using Amazon.S3;
using DotNet.Testcontainers.Images;
using Testcontainers.LocalStack;
using Xunit;

namespace OrchardCore.Tests.Integration.Infrastructure;

/// <summary>
/// Boots a LocalStack S3 emulator in a Docker container for the lifetime of the test
/// collection. LocalStack publishes a multi-architecture image (linux/amd64 and
/// linux/arm64), so Testcontainers pulls the variant matching the host CPU automatically;
/// no architecture-specific handling is required.
/// </summary>
public sealed class LocalStackFixture : IAsyncLifetime
{
    // LocalStack publishes a multi-architecture image (linux/amd64 and linux/arm64), so
    // Testcontainers pulls the variant matching the host CPU automatically; no
    // architecture-specific handling is required. The tag is pinned for reproducibility.
    private const string Image = "localstack/localstack:3.8.1";

    private LocalStackContainer _container;

    /// <summary>
    /// Gets the S3 service endpoint exposed by the running emulator, or <c>null</c> when
    /// Docker is unavailable and the container was not started.
    /// </summary>
    public string ServiceUrl { get; private set; }

    public async ValueTask InitializeAsync()
    {
        if (!DockerSupport.IsAvailable)
        {
            // Tests in this collection are skipped via [DockerFact]; skip the costly start.
            return;
        }

        _container = new LocalStackBuilder(new DockerImage(Image)).Build();
        await _container.StartAsync();
        ServiceUrl = _container.GetConnectionString();
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// Creates an <see cref="AmazonS3Client"/> configured to talk to the emulator using
    /// path-style addressing and dummy credentials.
    /// </summary>
    public AmazonS3Client CreateClient()
        => new(
            new BasicAWSCredentials("test", "test"),
            new AmazonS3Config
            {
                ServiceURL = ServiceUrl,
                ForcePathStyle = true,
                AuthenticationRegion = "us-east-1",
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
            });
}

/// <summary>
/// xUnit collection that shares a single <see cref="LocalStackFixture"/> across the
/// S3 integration test classes.
/// </summary>
[CollectionDefinition(Name)]
public sealed class LocalStackCollection : ICollectionFixture<LocalStackFixture>
{
    public const string Name = "LocalStack";
}
