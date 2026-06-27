using System.Buffers.Binary;
using NetVips;
using OrchardCore.Media;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.Media;

// End-to-end tests that exercise the real media image processing middleware, the NetVips engine,
// the token service and the resized-image cache through an in-memory tenant over TestServer.
public class MediaImageProcessingTests
{
    [Fact]
    public async Task ResizesImage_WithValidToken()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        await UploadImageAsync(context, "resize.png", 800, 400);
        var query = await CreateTokenizedQueryAsync(context, "/media/resize.png?width=240&height=120&rmode=max");

        var response = await context.Client.GetAsync($"media/resize.png?{query}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        var (width, height) = ReadPngSize(await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        Assert.Equal(240, width);
        Assert.Equal(120, height);
    }

    [Fact]
    public async Task ConvertsFormat_WithValidToken()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        await UploadImageAsync(context, "convert.png", 400, 200);
        var query = await CreateTokenizedQueryAsync(context, "/media/convert.png?width=240&format=webp");

        var response = await context.Client.GetAsync($"media/convert.png?{query}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        Assert.Equal("image/webp", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ServesOriginal_WithInvalidToken()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        await UploadImageAsync(context, "untrusted.png", 800, 400);

        // A forged token must be rejected and the original, unprocessed image served.
        var response = await context.Client.GetAsync("media/untrusted.png?width=240&height=120&rmode=max&token=forged", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var (width, height) = ReadPngSize(await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        Assert.Equal(800, width);
        Assert.Equal(400, height);
    }

    [Fact]
    public async Task ResizesImage_WithVersionAndToken()
    {
        // Regression guard: a "v" cache-buster must not be part of the HMAC, so a versioned and
        // tokenized URL must still be processed instead of silently serving the original.
        using var context = new SiteContext();
        await context.InitializeAsync();

        await UploadImageAsync(context, "versioned.png", 800, 400);
        var query = await CreateTokenizedQueryAsync(context, "/media/versioned.png?width=240&height=120&rmode=max");

        var response = await context.Client.GetAsync($"media/versioned.png?{query}&v=abc123", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var (width, height) = ReadPngSize(await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        Assert.Equal(240, width);
        Assert.Equal(120, height);
    }

    [Fact]
    public async Task ConcurrentFirstHits_AllSucceed()
    {
        // Regression guard for the cache stampede: many concurrent first-hit requests for the same
        // resized image must all succeed (atomic temp-file + rename), with no IOExceptions and no
        // truncated responses.
        using var context = new SiteContext();
        await context.InitializeAsync();

        await UploadImageAsync(context, "stampede.png", 800, 400);
        var query = await CreateTokenizedQueryAsync(context, "/media/stampede.png?width=480&height=240&rmode=max");
        var url = $"media/stampede.png?{query}";

        var tasks = Enumerable.Range(0, 32)
            .Select(_ => context.Client.GetAsync(url, TestContext.Current.CancellationToken))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            response.EnsureSuccessStatusCode();
            var (width, height) = ReadPngSize(await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
            Assert.Equal(480, width);
            Assert.Equal(240, height);
            response.Dispose();
        }
    }

    private static async Task UploadImageAsync(SiteContext context, string path, int width, int height)
    {
        using var image = Image.Black(width, height, bands: 3);
        var bytes = image.WriteToBuffer(".png");

        await context.UsingTenantScopeAsync(async scope =>
        {
            var fileStore = scope.ServiceProvider.GetRequiredService<IMediaFileStore>();
            using var stream = new MemoryStream(bytes);
            await fileStore.CreateFileFromStreamAsync(path, stream, overwrite: true);
        });
    }

    private static async Task<string> CreateTokenizedQueryAsync(SiteContext context, string pathWithQuery)
    {
        string tokenizedPath = null;

        await context.UsingTenantScopeAsync(scope =>
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<IMediaTokenService>();
            tokenizedPath = tokenService.AddTokenToPath(pathWithQuery);

            return Task.CompletedTask;
        });

        return tokenizedPath[(tokenizedPath.IndexOf('?') + 1)..];
    }

    private static (int Width, int Height) ReadPngSize(byte[] png)
    {
        // PNG: 8-byte signature, then a 4-byte length and "IHDR", then width (4 bytes) and height (4 bytes).
        Assert.True(png.Length > 24, "Response body is not a valid PNG.");
        var width = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4));
        var height = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4));

        return (width, height);
    }
}
