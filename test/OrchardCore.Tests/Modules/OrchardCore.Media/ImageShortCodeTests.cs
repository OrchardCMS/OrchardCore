using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.FileStorage;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Media.Core;
using OrchardCore.Media.Events;
using OrchardCore.Media.ShortCodes;
using OrchardCore.ShortCodes;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Media
{
    public class ImageShortCodeTests
    {
        [Theory]
        [InlineData("foo bar baz", "foo bar baz")]
        [InlineData("foo [media]bar baz", "foo [media]bar baz")]
        [InlineData("foo [media]bar[/media] baz", @"foo <img src=""/media/bar""> baz")]
        [InlineData("foo [media]bar[/media] baz foo [media]bar[/media] baz", @"foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz")]
        [InlineData("foo [media]bàr.jpeg?width=100[/media] baz", @"foo <img src=""/media/bàr.jpeg?width=100""> baz")]
        [InlineData("foo [media]bàr.jpeg?width=100 onload=\"javascript: alert('XSS')[/media] baz", @"foo <img src=""/media/bàr.jpeg?width=100 onload=""> baz")]
        [InlineData("foo [image]bar baz", "foo [image]bar baz")]
        [InlineData("foo [image]bar[/image] baz", @"foo <img src=""/media/bar""> baz")]
        [InlineData("foo [image]bar[/image] baz foo [image]bar[/image] baz", @"foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz")]
        [InlineData("foo [image]bar[/image] baz foo [image]bar[/image] baz foo [image]bar[/image] baz", @"foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz")]
        [InlineData("foo [image]bar.png[/image] baz foo-extended [image]bar-extended.png[/image] baz-extended", @"foo <img src=""/media/bar.png""> baz foo-extended <img src=""/media/bar-extended.png""> baz-extended")]
        [InlineData("foo [media]bar[/media] baz foo [image]bar[/image] baz", @"foo <img src=""/media/bar""> baz foo <img src=""/media/bar""> baz")]
        [InlineData("foo [image]bàr.jpeg?width=100[/image] baz", @"foo <img src=""/media/bàr.jpeg?width=100""> baz")]
        [InlineData("foo [image]bàr.jpeg?width=100 onload=\"javascript: alert('XSS')[/image] baz", @"foo <img src=""/media/bàr.jpeg?width=100 onload=""> baz")]

        public async Task ShouldProcess(string text, string expected)
        {
            var mediaShortCode = new ImageShortCode(
            new DefaultMediaFileStore(Mock.Of<IFileStore>(),
                "/media",
                string.Empty,
                Enumerable.Empty<IMediaEventHandler>(),
                Enumerable.Empty<IMediaCreatingEventHandler>(),
                Mock.Of<ILogger<DefaultMediaFileStore>>()),
            new HtmlSanitizerService(Options.Create(new HtmlSanitizerOptions()))
        );

            var shortCodeContext = new ShortCodeContext(mediaShortCode.Name);
            var shortCodeOutput = new ShortCodeOutput(mediaShortCode.Name, encoder =>
            {
                var shortCodeContent = new DefaultShortCodeContent();
                shortCodeContent.SetHtmlContent(text);

                return Task.FromResult<ShortCodeContent>(shortCodeContent);
            });

            await mediaShortCode.ProcessAsync(shortCodeContext, shortCodeOutput);

            using (var writer = new StringWriter())
            {
                shortCodeOutput.WriteTo(writer, NullHtmlEncoder.Default);

                Assert.Equal(expected, writer.ToString());
            }
        }
    }
}
