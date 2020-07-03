using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.FileStorage;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Media.Core;
using OrchardCore.Media.Events;
using OrchardCore.Media.Shortcodes;
using OrchardCore.Shortcodes.Services;
using Shortcodes;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Media
{
    public class ImageShortcodeTests
    {
        [Theory]
        [InlineData("foo bar baz", "foo bar baz")]
        [InlineData("foo [media]bar[/media] baz", @"foo <img src=""/media/bar""> baz")]
        [InlineData("foo [media]bar[/media] baz foo [media]bar[/media] baz", @"foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz")]
        [InlineData("foo [media]bàr.jpeg?width=100[/media] baz", @"foo <img src=""/media/bàr.jpeg?width=100""> baz")]
        [InlineData("foo [media]bàr.jpeg?width=100 onload=\"javascript: alert('XSS')[/media] baz", @"foo <img src=""/media/bàr.jpeg?width=100 onload=""> baz")]
        [InlineData("foo [image]bar baz", "foo [image]bar baz")]
        [InlineData(@"foo [image ""bar""] baz", @"foo <img src=""/media/bar""> baz")]
        [InlineData(@"foo [image src=""bar""] baz", @"foo <img src=""/media/bar""> baz")]

        // [InlineData("foo [image]~/bar[/image] baz", @"foo <img src=""/bar""> baz")]
        [InlineData("foo [image]http://bar[/image] baz", @"foo <img src=""http://bar""> baz")]
        [InlineData("foo [image]//bar[/image] baz", @"foo <img src=""//bar""> baz")]
        [InlineData("foo [image]bar[/image] baz", @"foo <img src=""/media/bar""> baz")]
        [InlineData(@"foo [image width=""100""]bar[/image] baz", @"foo <img src=""/media/bar?width=100""> baz")]
        [InlineData(@"foo [image width=""100"" height=""50"" mode=""stretch""]bar[/image] baz", @"foo <img src=""/media/bar?width=100&amp;height=50&amp;rmode=stretch""> baz")]
        [InlineData("foo [image]bar[/image] baz foo [image]bar[/image] baz", @"foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz")]
        [InlineData("foo [image]bar[/image] baz foo [image]bar[/image] baz foo [image]bar[/image] baz", @"foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz")]
        [InlineData("foo [image]bar.png[/image] baz foo-extended [image]bar-extended.png[/image] baz-extended", @"foo <img src=""/media/bar.png""> baz foo-extended <img src=""/media/bar-extended.png""> baz-extended")]
        [InlineData("foo [media]bar[/media] baz foo [image]bar[/image] baz", @"foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz")]
        [InlineData("foo [image]bàr.jpeg?width=100[/image] baz", @"foo <img src=""/media/bàr.jpeg?width=100""> baz")]
        [InlineData("foo [image]bàr.jpeg?width=100 onload=\"javascript: alert('XSS')[/image] baz", @"foo <img src=""/media/bàr.jpeg?width=100 onload=""> baz")]

        public async Task ShouldProcess(string text, string expected)
        {
            var imageProvider = new ImageShortcodeProvider(
                new DefaultMediaFileStore(Mock.Of<IFileStore>(),
                    "/media",
                    string.Empty,
                    Enumerable.Empty<IMediaEventHandler>(),
                    Enumerable.Empty<IMediaCreatingEventHandler>(),
                    Mock.Of<ILogger<DefaultMediaFileStore>>()),
                new HtmlSanitizerService(Options.Create(new HtmlSanitizerOptions()))
            );

            var processor = new ShortcodeService(new IShortcodeProvider[] { imageProvider });

            var processed = await processor.ProcessAsync(text);
            Assert.Equal(expected, processed);
        }
    }
}
