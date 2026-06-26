using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.GraphQL;

public class MediaFieldResizeUrlQueryTests
{
    [Fact]
    public async Task ResizeUrlShouldReturnAUrlThatServesAProcessedImage()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

        // The blog content item created by the Blog recipe has an "Image" media field
        // (the banner, "home-bg.jpg"). Query its resizeUrl through GraphQL.
        var result = await context
            .GraphQLClient
            .Content
            .Query("blog { image { files { url resizeUrl(width: 100, height: 80, mode: \"max\") } } }");

        var file = result["data"]?["blog"]?[0]?["image"]?["files"]?[0];

        Assert.NotNull(file);

        var resizeUrl = file["resizeUrl"]?.ToString();

        Assert.False(string.IsNullOrEmpty(resizeUrl));

        // The URL must carry the resize commands; when media token validation is enabled it must
        // also carry a token so the middleware accepts the request.
        Assert.Contains("width=100", resizeUrl);
        Assert.Contains("height=80", resizeUrl);

        // Fetch the generated URL and confirm the image-processing middleware (NetVips by default)
        // serves a processed image, proving the GraphQL-produced URL is consumable end to end.
        var response = await context.Client.GetAsync(resizeUrl, TestContext.Current.CancellationToken);

        Assert.True(
            response.IsSuccessStatusCode,
            $"Expected success for '{resizeUrl}' but got {(int)response.StatusCode} {response.StatusCode}.");

        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.StartsWith("image/", response.Content.Headers.ContentType.MediaType);

        var bytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        Assert.NotEmpty(bytes);
    }
}
