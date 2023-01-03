using OrchardCore.FileStorage;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Media.Core;
using OrchardCore.Media.Events;
using OrchardCore.Media.Shortcodes;
using OrchardCore.ResourceManagement;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Tests.Modules.OrchardCore.Media
{
    public class AssetUrlShortcodeTests
    {
        [Theory]
        [InlineData("", "foo [asset_url]bar baz", "foo [asset_url]bar baz")]
        [InlineData("", @"foo [asset_url ""bar""] baz", @"foo /media/bar baz")]
        [InlineData("", @"foo [asset_url src=""bar""] baz", @"foo /media/bar baz")]
        [InlineData("https://cdn.com", @"foo [asset_url src=""bar""] baz", @"foo https://cdn.com/media/bar baz")]
        [InlineData("", "foo [asset_url]~/bar[/asset_url] baz", @"foo /tenant/bar baz")]
        [InlineData("https://cdn.com", "foo [asset_url]~/bar[/asset_url] baz", @"foo https://cdn.com/tenant/bar baz")]
        [InlineData("", "foo [asset_url]http://bar[/asset_url] baz", @"foo http://bar baz")]
        [InlineData("", "foo [asset_url]//bar[/asset_url] baz", @"foo //bar baz")]
        [InlineData("", "foo [asset_url]bar[/asset_url] baz", @"foo /media/bar baz")]
        [InlineData("", @"foo [asset_url width=""100""]bar[/asset_url] baz", @"foo /media/bar?width=100 baz")]
        [InlineData("", @"foo [asset_url width=""100"" height=""50"" mode=""stretch""]bar[/asset_url] baz", @"foo /media/bar?width=100&amp;height=50&amp;rmode=stretch baz")]
        [InlineData("", "foo [asset_url]bar[/asset_url] baz foo [asset_url]bar[/asset_url] baz", @"foo /media/bar baz foo /media/bar baz")]
        [InlineData("", @"foo <a href=""[asset_url]bàr.jpeg onload=""javascript: alert('XSS')""[/asset_url]"">baz</a>", @"foo <a href=""/media/bàr.jpeg onload="">baz</a>")]
        [InlineData("", @"foo <a href=""[asset_url]bàr.jpeg?width=100 onload=""javascript: alert('XSS')""[/asset_url]"">baz</a>", @"foo <a href=""/media/bàr.jpeg?width=100 onload="">baz</a>")]
        public async Task ShouldProcess(string cdnBaseUrl, string text, string expected)
        {
            var fileStore = new DefaultMediaFileStore(
                Mock.Of<IFileStore>(),
                "/media",
                cdnBaseUrl,
                Enumerable.Empty<IMediaEventHandler>(),
                Enumerable.Empty<IMediaCreatingEventHandler>(),
                Mock.Of<ILogger<DefaultMediaFileStore>>());

            var sanitizer = new HtmlSanitizerService(Options.Create(new HtmlSanitizerOptions()));

            var defaultHttpContext = new DefaultHttpContext();
            defaultHttpContext.Request.PathBase = new PathString("/tenant");
            var httpContextAccessor = Mock.Of<IHttpContextAccessor>(hca => hca.HttpContext == defaultHttpContext);

            var options = Options.Create(new ResourceManagementOptions { CdnBaseUrl = cdnBaseUrl });

            var assetUrlProvider = new AssetUrlShortcodeProvider(fileStore, httpContextAccessor, options);

            var processor = new ShortcodeService(new IShortcodeProvider[] { assetUrlProvider }, Enumerable.Empty<IShortcodeContextProvider>());

            var processed = await processor.ProcessAsync(text);
            // The markdown part sanitizes after processing.
            var sanitized = sanitizer.Sanitize(processed);
            Assert.Equal(expected, sanitized);
        }

        [Theory]
        [InlineData(@"foo <a href=""[asset_url]bàr.jpeg[/asset_url]"">baz</a>", @"foo <a href=""[asset_url]bàr.jpeg[/asset_url]"">baz</a>")]
        // Sanitizing strips the closing shortcode tag when it finds onload. This is fine, as the user cannot expect a good shortcode when they attempt xss
        [InlineData(@"foo <a href=""[asset_url]bàr.jpeg?width=100 onload=""javascript: alert('XSS')""[/asset_url]"">baz</a>", @"foo <a href=""[asset_url]bàr.jpeg?width=100 onload="">baz</a>")]
        public void ShouldSanitizeUnprocessed(string text, string expected)
        {
            // The html parts santize on save, so do not process the shortcode first.
            var sanitizer = new HtmlSanitizerService(Options.Create(new HtmlSanitizerOptions()));
            var sanitized = sanitizer.Sanitize(text);
            Assert.Equal(expected, sanitized);
        }
    }
}
