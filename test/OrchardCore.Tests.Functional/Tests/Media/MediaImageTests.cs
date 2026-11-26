using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using OrchardCore.Tests.Functional.Tests.Cms;

namespace OrchardCore.Tests.Functional.Tests.Media;

/// <summary>
/// End-to-end tests for media image processing, tag helper URL generation, and middleware pipeline.
/// Uses the Blog recipe fixture which includes demo content with media images.
/// </summary>
public sealed class MediaImageTests : CmsTestBase<BlogFixture>, IClassFixture<BlogFixture>
{
    public MediaImageTests(BlogFixture fixture) : base(fixture) { }

    [Fact]
    public async Task BlogPage_RendersMediaImages_Succeeds()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAndAssertOkAsync("/");

        Assert.Contains("/media/", await GetMastheadImageUrlAsync(page));

        await page.CloseAsync();
    }

    [Fact]
    public async Task MediaEndpoint_SupportedWidth_ReturnsImage()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAndAssertOkAsync("/");

        var src = await GetMastheadImageUrlAsync(page);

        // Strip existing query params so we can add our own supported size.
        var pathOnly = src.Contains('?') ? src[..src.IndexOf('?')] : src;

        // 600 is in the default SupportedSizes — valid without a token.
        var response = await page.GotoAsync($"{pathOnly}?width=600");

        Assert.NotNull(response);
        Assert.True(response!.Ok, $"Expected HTTP 200 for resized media URL, got {response.Status}.");
        Assert.StartsWith("image/", response.Headers.GetValueOrDefault("content-type", ""));

        await page.CloseAsync();
    }

    [Fact]
    public async Task MediaImages_RenderedWithResizeParams_ContainSecurityToken()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAndAssertOkAsync("/");

        var src = await GetMastheadImageUrlAsync(page);

        await page.CloseAsync();

        Assert.Contains("width=", src);
        Assert.Contains("token=", src);
    }

    [Fact]
    public async Task MediaImages_RenderedWithToken_ReturnsValidImageResponses()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAndAssertOkAsync("/");

        var src = await GetMastheadImageUrlAsync(page);

        var response = await page.GotoAsync(src);

        Assert.NotNull(response);
        Assert.True(response!.Ok,
            $"Expected resized image URL to return HTTP 200, got {response.Status}: {src}");
        Assert.StartsWith("image/", response.Headers.GetValueOrDefault("content-type", ""));

        await page.CloseAsync();
    }

    [Fact]
    public async Task MediaEndpoint_UnsupportedSizeWithoutToken_ServesOriginalImage()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAndAssertOkAsync("/");

        var src = await GetMastheadImageUrlAsync(page);

        var pathOnly = src.Contains('?') ? src[..src.IndexOf('?')] : src;

        // 999 is not in SupportedSizes — parser strips it, middleware passes through to static files.
        var response = await page.GotoAsync($"{pathOnly}?width=999");

        Assert.NotNull(response);
        Assert.True(response!.Ok,
            $"Expected 200 (original image served) for unsupported size, got {response.Status}.");
        Assert.StartsWith("image/", response.Headers.GetValueOrDefault("content-type", ""));

        await page.CloseAsync();
    }

    // The Blog theme shows its media image as the background of the masthead, not in an 'img' element.
    // The computed value is 'url("...")', with a fully-qualified URL to navigate to.
    private static Task<string> GetMastheadImageUrlAsync(IPage page)
        => page.Locator("header.masthead").EvaluateAsync<string>("header => getComputedStyle(header).backgroundImage.slice(5, -2)");
}
