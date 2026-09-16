using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment.Artifacts;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class DeploymentArtifactStoreTests
{
    [Fact]
    public async Task StoredArtifact_PersistsAcrossInstancesAndChecksTenantOwnerAndChecksum()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        try
        {
            var now = DateTime.UtcNow;
            var store = Store(root, "one", () => now);
            var bytes = Encoding.UTF8.GetBytes("artifact bytes");
            using var input = new MemoryStream(bytes);
            var artifact = await store.CreateAsync("client:one", DeploymentArtifactKind.Export, "package.zip", "application/zip", input, TestContext.Current.CancellationToken);
            Assert.True(input.CanRead);
            Assert.Equal(bytes.Length, artifact.Length);
            Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(bytes)), artifact.Sha256);
            var reopened = Store(root, "one", () => now);
            Assert.NotNull(await reopened.FindAsync(artifact.Id, "client:one"));
            Assert.Null(await reopened.FindAsync(artifact.Id, "client:other"));
            Assert.Null(await Store(root, "two", () => now).FindAsync(artifact.Id, "client:one"));
            Assert.Null(await reopened.OpenAsync("../metadata", "client:one"));
            Assert.Equal(ArtifactDeleteResult.Missing, await reopened.DeleteAsync(artifact.Id, "client:other"));
            using (var lease = await reopened.OpenAsync(artifact.Id, "client:one"))
            {
                Assert.NotNull(lease);
                using var reader = new StreamReader(lease.Stream, leaveOpen: true);
                Assert.Equal("artifact bytes", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
                Assert.Equal(ArtifactDeleteResult.Busy, await store.DeleteAsync(artifact.Id, "client:one"));
            }
            Assert.Equal(ArtifactDeleteResult.Deleted, await store.DeleteAsync(artifact.Id, "client:one"));
            Assert.Equal(ArtifactDeleteResult.Missing, await store.DeleteAsync(artifact.Id, "client:one"));
            Assert.Null(await reopened.FindAsync(artifact.Id, "client:one"));
        }
        finally
        {
            if (Directory.Exists(root)) { Directory.Delete(root, recursive: true); }
        }
    }

    [Fact]
    public async Task ExpiryCleanup_SkipsOpenLeaseThenRemovesExpiredArtifact()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        try
        {
            var now = DateTime.UtcNow;
            var store = Store(root, "one", () => now);
            using var input = new MemoryStream([1, 2, 3]);
            var artifact = await store.CreateAsync("user:one", DeploymentArtifactKind.Export, "package.zip", "application/zip", input, TestContext.Current.CancellationToken);
            using (var lease = await store.OpenAsync(artifact.Id, "user:one"))
            {
                Assert.NotNull(lease);
                now += TimeSpan.FromDays(2);
                Assert.Null(await store.FindAsync(artifact.Id, "user:one"));
                Assert.Null(await store.OpenAsync(artifact.Id, "user:one"));
                Assert.Equal(0, await Store(root, "one", () => now).CleanupAsync(cancellationToken: TestContext.Current.CancellationToken));
                Assert.True(lease.Stream.CanRead);
            }
            Assert.Equal(1, await store.CleanupAsync(cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(0, await store.CleanupAsync(cancellationToken: TestContext.Current.CancellationToken));
        }
        finally
        {
            if (Directory.Exists(root)) { Directory.Delete(root, recursive: true); }
        }
    }

    [Fact]
    public async Task ScheduledCleanup_UsesTenantScopeAndHonorsCancellation()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        try
        {
            var now = DateTime.UtcNow;
            var one = Store(root, "one", () => now);
            var two = Store(root, "two", () => now);
            using var firstInput = new MemoryStream([1]);
            using var secondInput = new MemoryStream([2]);
            var first = await one.CreateAsync("owner", DeploymentArtifactKind.Import, "recipe.json", "application/json", firstInput, TestContext.Current.CancellationToken);
            var second = await two.CreateAsync("owner", DeploymentArtifactKind.Import, "recipe.json", "application/json", secondInput, TestContext.Current.CancellationToken);
            now += TimeSpan.FromDays(2);
            using var services = new ServiceCollection().AddSingleton(one).BuildServiceProvider();
            var task = new DeploymentArtifactCleanupTask();
            using var cancelled = new CancellationTokenSource();
            await cancelled.CancelAsync();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task.DoWorkAsync(services, cancelled.Token));
            Assert.NotNull(await one.FindAsync(first.Id, "owner", includeExpired: true));
            await task.DoWorkAsync(services, TestContext.Current.CancellationToken);
            Assert.Null(await one.FindAsync(first.Id, "owner", includeExpired: true));
            Assert.NotNull(await two.FindAsync(second.Id, "owner", includeExpired: true));
        }
        finally
        {
            if (Directory.Exists(root)) { Directory.Delete(root, recursive: true); }
        }
    }

    [Fact]
    public async Task SizeLimit_RemovesUnpublishedArtifact()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        try
        {
            var store = Store(root, "one", () => DateTime.UtcNow, new DeploymentArtifactOptions { MaxBytes = 2 });
            using var input = new MemoryStream([1, 2, 3]);
            await Assert.ThrowsAsync<InvalidDataException>(() => store.CreateAsync("client:one", DeploymentArtifactKind.Export, "package.zip", "application/zip", input, TestContext.Current.CancellationToken));
            Assert.Empty(Directory.GetFiles(root, "*", SearchOption.AllDirectories));
        }
        finally
        {
            if (Directory.Exists(root)) { Directory.Delete(root, recursive: true); }
        }
    }

    [Fact]
    public async Task OrphanCleanup_SkipsActiveWriterAndRemovesAbandonedFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        try
        {
            var now = DateTime.UtcNow;
            var folder = Path.Combine(root, "Sites", "one", "DeploymentArtifacts", Guid.NewGuid().ToString("n"));
            Directory.CreateDirectory(folder);
            await File.WriteAllTextAsync(Path.Combine(folder, "content"), "unfinished", TestContext.Current.CancellationToken);
            var store = Store(root, "one", () => now);
            using (var guard = new FileStream(Path.Combine(folder, "lease"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
                Directory.SetLastWriteTimeUtc(folder, now - TimeSpan.FromDays(2));
                Assert.Equal(0, await store.CleanupAsync(cancellationToken: TestContext.Current.CancellationToken));
                Assert.True(File.Exists(Path.Combine(folder, "content")));
            }
            Assert.Equal(1, await store.CleanupAsync(cancellationToken: TestContext.Current.CancellationToken));
            Assert.False(Directory.Exists(folder));
        }
        finally
        {
            if (Directory.Exists(root)) { Directory.Delete(root, recursive: true); }
        }
    }

    private static DeploymentArtifactStore Store(string root, string tenant, Func<DateTime> now, DeploymentArtifactOptions options = null)
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(value => value.UtcNow).Returns(now);
        return new DeploymentArtifactStore(Options.Create(new ShellOptions { ShellsApplicationDataPath = root, ShellsContainerName = "Sites" }),
            new ShellSettings { Name = tenant }, Options.Create(options ?? new()), clock.Object);
    }
}
