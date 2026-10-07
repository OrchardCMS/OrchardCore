using DotNet.Testcontainers.Images;
using Testcontainers.Azurite;
using Xunit;

namespace OrchardCore.Tests.Integration.Infrastructure;

/// <summary>
/// Boots an Azurite Azure Storage emulator in a Docker container for the lifetime of the
/// test collection. The Azurite image is multi-architecture (linux/amd64 and linux/arm64),
/// so Testcontainers pulls the variant matching the host CPU automatically; no
/// architecture-specific handling is required.
/// </summary>
public sealed class AzuriteFixture : IAsyncLifetime
{
    // The Azurite image is multi-architecture (linux/amd64 and linux/arm64), so
    // Testcontainers pulls the variant matching the host CPU automatically; no
    // architecture-specific handling is required. The tag is pinned for reproducibility.
    private const string Image = "mcr.microsoft.com/azure-storage/azurite:3.33.0";

    private AzuriteContainer _container;

    /// <summary>
    /// Gets the Azure Storage connection string for the running emulator, or <c>null</c>
    /// when Docker is unavailable and the container was not started.
    /// </summary>
    public string ConnectionString { get; private set; }

    public async ValueTask InitializeAsync()
    {
        if (!DockerSupport.IsAvailable)
        {
            // Tests in this collection are skipped via [DockerFact]; skip the costly start.
            return;
        }

        // The Azure Blob SDK negotiates a very recent REST API version that a pinned Azurite
        // image may not recognize. --skipApiVersionCheck makes the emulator accept any version,
        // which keeps the tests stable across SDK upgrades. WithCommand appends to the module's
        // default host-binding arguments.
        _container = new AzuriteBuilder(new DockerImage(Image))
            .WithCommand("--skipApiVersionCheck")
            .Build();
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}

/// <summary>
/// xUnit collection that shares a single <see cref="AzuriteFixture"/> across the
/// Azure Blob integration test classes.
/// </summary>
[CollectionDefinition(Name)]
public sealed class AzuriteCollection : ICollectionFixture<AzuriteFixture>
{
    public const string Name = "Azurite";
}
