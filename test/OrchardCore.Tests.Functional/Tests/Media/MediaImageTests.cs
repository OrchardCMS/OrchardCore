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

        var count = await page.Locator("img[src*='/media/']").CountAsync();
        Assert.True(count > 0, "Expected at least one media image on the blog homepage.");

        await page.CloseAsync();
    }

    [Fact]
    public async Task MediaEndpoint_SupportedWidth_ReturnsImage()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAndAssertOkAsync("/");

        // Get a media image URL. Use img.src (fully-qualified) for navigation.
        var src = await page.Locator("img[src*='/media/']").First
            .EvaluateAsync<string>("img => img.src");

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

        // Collect src attributes of images that already have resize parameters.
        var srcs = await page.Locator("img[src*='/media/'][src*='width=']")
            .EvaluateAllAsync<string[]>("imgs => imgs.map(img => img.getAttribute('src')).filter(Boolean)");

        await page.CloseAsync();

        if (srcs.Length == 0)
        {
            // The current theme doesn't use resize tag helpers on the home page; skip.
            return;
        }

        foreach (var src in srcs)
        {
            Assert.True(src.Contains("token=", StringComparison.Ordinal),
                $"Image URL rendered with resize params must include a security token: {src}");
        }
    }

    [Fact]
    public async Task MediaImages_RenderedWithToken_ReturnsValidImageResponses()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAndAssertOkAsync("/");

        // Collect fully-qualified URLs of images with resize + token.
        var srcs = await page.Locator("img[src*='/media/'][src*='width='][src*='token=']")
            .EvaluateAllAsync<string[]>("imgs => imgs.map(img => img.src)");

        await page.CloseAsync();

        if (srcs.Length == 0)
        {
            // Theme doesn't render tokenized resize URLs on this page; skip.
            return;
        }

        foreach (var src in srcs.Take(3))
        {
            var imgPage = await Fixture.CreatePageAsync();
            var response = await imgPage.GotoAsync(src);
            Assert.NotNull(response);
            Assert.True(response!.Ok,
                $"Expected resized image URL to return HTTP 200, got {response.Status}: {src}");
            Assert.True(response.Headers.GetValueOrDefault("content-type", "").StartsWith("image/", StringComparison.OrdinalIgnoreCase),
                $"Expected image content-type for {src}");
            await imgPage.CloseAsync();
        }
    }

    [Fact]
    public async Task MediaEndpoint_UnsupportedSizeWithoutToken_ServesOriginalImage()
    {
        var page = await Fixture.CreatePageAsync();
        await page.GotoAndAssertOkAsync("/");

        var src = await page.Locator("img[src*='/media/']").First
            .EvaluateAsync<string>("img => img.src");

        var pathOnly = src.Contains('?') ? src[..src.IndexOf('?')] : src;

        // 999 is not in SupportedSizes — parser strips it, middleware passes through to static files.
        var response = await page.GotoAsync($"{pathOnly}?width=999");

        Assert.NotNull(response);
        Assert.True(response!.Ok,
            $"Expected 200 (original image served) for unsupported size, got {response.Status}.");
        Assert.StartsWith("image/", response.Headers.GetValueOrDefault("content-type", ""));

        await page.CloseAsync();
    }
}
