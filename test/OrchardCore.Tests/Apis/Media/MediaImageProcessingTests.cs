using System.Buffers.Binary;
using OrchardCore.Media;
using OrchardCore.Tests.Apis.Context;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OrchardCore.Tests.Apis.Media;

// End-to-end tests that exercise the real media image processing middleware (ImageSharp.Web),
// the token service and the resized-image cache through an in-memory tenant over TestServer.
public class MediaImageProcessingTests
{
    [Fact]
    public async Task ResizesImage_ValidToken_Succeeds()
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
    public async Task ConvertsFormat_ValidToken_Succeeds()
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
    public async Task ServesOriginal_InvalidToken_Succeeds()
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
    public async Task ConcurrentFirstHits_AllSucceed_Succeeds()
    {
        // Regression guard for the cache stampede: many concurrent first-hit requests for the same
        // resized image must all succeed, with no IOExceptions and no truncated responses.
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
        using var image = new Image<Rgb24>(width, height);
        using var buffer = new MemoryStream();
        await image.SaveAsPngAsync(buffer);
        var bytes = buffer.ToArray();

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
