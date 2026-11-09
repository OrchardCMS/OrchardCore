#nullable enable

using Microsoft.AspNetCore.Hosting;
using OrchardCore.Environment.Shell;
using OrchardCore.Media.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public sealed class PhysicalFileSystemResizedImageCacheTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("resized-image-cache-tests").FullName;

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("static")]
    public async Task SetAsync_MissingOrCustomWebRoot_CachesAndServesImage(string? webRoot)
    {
        var webRootPath = string.IsNullOrEmpty(webRoot) ? webRoot : Path.Combine(_root, webRoot);
        var environment = Mock.Of<IWebHostEnvironment>(env =>
            env.ContentRootPath == _root && env.WebRootPath == webRootPath);
        using var shellSettings = new ShellSettings { Name = "Tenant1" };
        var cache = new PhysicalFileSystemResizedImageCache(
            environment, shellSettings, NullLogger<PhysicalFileSystemResizedImageCache>.Instance);
        var cancellationToken = TestContext.Current.CancellationToken;
        using var image = new MemoryStream(Encoding.UTF8.GetBytes("cached image"));

        Assert.Null(await cache.GetAsync("abcdef", ".png", cancellationToken));

        await cache.SetAsync("abcdef", image, "image/png", TimeSpan.FromDays(1), cancellationToken);

        var expectedPath = Path.Combine(_root, string.IsNullOrEmpty(webRoot) ? "wwwroot" : webRoot,
            "Tenant1", "media-cache", "ab", "abcdef.png");
        Assert.True(File.Exists(expectedPath));

        var cached = await cache.GetAsync("abcdef", ".png", cancellationToken);
        Assert.NotNull(cached);
        Assert.Equal("image/png", cached.Value.ContentType);
        using var reader = new StreamReader(cached.Value.Content);
        Assert.Equal("cached image", await reader.ReadToEndAsync(cancellationToken));
    }

    public void Dispose()
    {
        Directory.Delete(_root, recursive: true);
    }
}
