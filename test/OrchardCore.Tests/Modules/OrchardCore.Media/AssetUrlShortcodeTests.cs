using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.FileStorage;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Media.Core;
using OrchardCore.Media.Events;
using OrchardCore.Media.Shortcodes;
using OrchardCore.ResourceManagement;
using OrchardCore.Shortcodes.Services;
using Shortcodes;
using Xunit;

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
        [InlineData("", "foo [asset_url]bàr.jpeg?width=100 onload=\"javascript: alert('XSS')\"[/asset_url] baz", @"foo /media/bàr.jpeg?width=100 onload= baz")]
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

            var assetUrlProvider = new AssetUrlShortcodeProvider(fileStore, sanitizer, httpContextAccessor, options);

            var processor = new ShortcodeService(new IShortcodeProvider[] { assetUrlProvider }, Enumerable.Empty<IShortcodeContextProvider>());

            var processed = await processor.ProcessAsync(text);
            Assert.Equal(expected, processed);
        }
    }
}
