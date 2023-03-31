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
    public class ImageShortcodeTests
    {
        [Theory]
        [InlineData("", "foo bar baz", "foo bar baz")]
        [InlineData("", "foo [media]bar[/media] baz", @"foo <img src=""/media/bar""> baz")]
        [InlineData("", "foo [media]bar[/media] baz foo [media]bar[/media] baz", @"foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz")]
        [InlineData("", "foo [media]bàr.jpeg?width=100[/media] baz", @"foo <img src=""/media/bàr.jpeg?width=100""> baz")]
        [InlineData("", "foo [media]bàr.jpeg?width=100 onload=\"javascript: alert('XSS')[/media] baz", @"foo <img src=""/media/bàr.jpeg?width=100 onload=""> baz")]
        [InlineData("", "foo [image]bar baz", "foo [image]bar baz")]
        [InlineData("", @"foo [image ""bar""] baz", @"foo <img src=""/media/bar""> baz")]
        [InlineData("", @"foo [image src=""bar""] baz", @"foo <img src=""/media/bar""> baz")]
        [InlineData("https://cdn.com", @"foo [image src=""bar""] baz", @"foo <img src=""https://cdn.com/media/bar""> baz")]
        [InlineData("", "foo [image]~/bar[/image] baz", @"foo <img src=""/tenant/bar""> baz")]
        [InlineData("https://cdn.com", "foo [image]~/bar[/image] baz", @"foo <img src=""https://cdn.com/tenant/bar""> baz")] // new
        [InlineData("", "foo [image]http://bar[/image] baz", @"foo <img src=""http://bar""> baz")]
        [InlineData("", "foo [image]//bar[/image] baz", @"foo <img src=""//bar""> baz")]
        [InlineData("", "foo [image]bar[/image] baz", @"foo <img src=""/media/bar""> baz")]
        [InlineData("", @"foo [image width=""100""]bar[/image] baz", @"foo <img src=""/media/bar?width=100""> baz")]
        [InlineData("", @"foo [image width=""100"" height=""50"" mode=""stretch""]bar[/image] baz", @"foo <img src=""/media/bar?width=100&amp;height=50&amp;rmode=stretch""> baz")]
        [InlineData("", @"foo [image class=""className""]bar[/image] baz", @"foo <img class=""className"" src=""/media/bar""> baz")]
        [InlineData("", @"foo [image alt=""text""]bar[/image] baz", @"foo <img alt=""text"" src=""/media/bar""> baz")]
        [InlineData("", @"foo [image width=""100"" height=""50"" mode=""stretch"" alt=""text"" class=""className""]bar[/image] baz", @"foo <img alt=""text"" class=""className"" src=""/media/bar?width=100&amp;height=50&amp;rmode=stretch""> baz")]
        [InlineData("", "foo [image]bar[/image] baz foo [image]bar[/image] baz", @"foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz")]
        [InlineData("", "foo [image]bar[/image] baz foo [image]bar[/image] baz foo [image]bar[/image] baz", @"foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz")]
        [InlineData("", "foo [image]bar.png[/image] baz foo-extended [image]bar-extended.png[/image] baz-extended", @"foo <img src=""/media/bar.png""> baz foo-extended <img src=""/media/bar-extended.png""> baz-extended")]
        [InlineData("", "foo [media]bar[/media] baz foo [image]bar[/image] baz", @"foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz")]
        [InlineData("", "foo [image]bàr.jpeg?width=100[/image] baz", @"foo <img src=""/media/bàr.jpeg?width=100""> baz")]
        [InlineData("", "foo [image]bàr.jpeg?width=100 onload=\"javascript: alert('XSS')\"[/image] baz", @"foo <img src=""/media/bàr.jpeg?width=100 onload=""> baz")]
        public async Task ShouldProcess(string cdnBaseUrl, string text, string expected)
        {
            var sanitizerOptions = new HtmlSanitizerOptions();
            sanitizerOptions.Configure.Add(opt => opt.AllowedAttributes.Add("class"));

            var fileStore = new DefaultMediaFileStore(
                Mock.Of<IFileStore>(),
                "/media",
                cdnBaseUrl,
                Enumerable.Empty<IMediaEventHandler>(),
                Enumerable.Empty<IMediaCreatingEventHandler>(),
                Mock.Of<ILogger<DefaultMediaFileStore>>());

            var sanitizer = new HtmlSanitizerService(Options.Create(sanitizerOptions));

            var defaultHttpContext = new DefaultHttpContext();
            defaultHttpContext.Request.PathBase = new PathString("/tenant");
            var httpContextAccessor = Mock.Of<IHttpContextAccessor>(hca => hca.HttpContext == defaultHttpContext);

            var options = Options.Create(new ResourceManagementOptions { CdnBaseUrl = cdnBaseUrl });

            var imageProvider = new ImageShortcodeProvider(fileStore, sanitizer, httpContextAccessor, options);

            var processor = new ShortcodeService(new IShortcodeProvider[] { imageProvider }, Enumerable.Empty<IShortcodeContextProvider>());

            var processed = await processor.ProcessAsync(text);
            Assert.Equal(expected, processed);
        }
    }
}
