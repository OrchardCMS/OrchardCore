#nullable enable

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Media;
using OrchardCore.Media.Core;
using OrchardCore.Media.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public sealed class RemoteMediaCacheBackgroundTaskTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("remote-media-cache-tests").FullName;

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("static")]
    public async Task DoWorkAsync_MissingOrCustomWebRoot_CleansStaleCache(string? webRoot)
    {
        var webRootPath = string.IsNullOrEmpty(webRoot) ? webRoot : Path.Combine(_root, webRoot);
        var environment = Mock.Of<IWebHostEnvironment>(env =>
            env.ContentRootPath == _root && env.WebRootPath == webRootPath);
        using var shellSettings = new ShellSettings { Name = "Tenant1" };
        var task = new RemoteMediaCacheBackgroundTask(
            shellSettings, Mock.Of<IMediaFileStore>(), environment,
            Options.Create(new MediaOptions { RemoteCacheMaxStale = TimeSpan.FromDays(1) }),
            NullLogger<RemoteMediaCacheBackgroundTask>.Instance);
        using var services = new ServiceCollection()
            .AddSingleton(Mock.Of<IMediaFileStoreCache>())
            .BuildServiceProvider();
        var cachePath = Path.Combine(_root, string.IsNullOrEmpty(webRoot) ? "wwwroot" : webRoot,
            "Tenant1", DefaultMediaFileStoreCacheFileProvider.AssetsCachePath);
        Directory.CreateDirectory(cachePath);
        var filePath = Path.Combine(cachePath, "stale.png");
        var cancellationToken = TestContext.Current.CancellationToken;
        await File.WriteAllTextAsync(filePath, "stale image", cancellationToken);
        File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow.AddDays(-2));

        await task.DoWorkAsync(services, cancellationToken);

        Assert.False(File.Exists(filePath));
    }

    public void Dispose()
    {
        Directory.Delete(_root, recursive: true);
    }
}
