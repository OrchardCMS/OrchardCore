using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.FileStorage;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Media.Core;
using OrchardCore.Media.Events;
using OrchardCore.Media.ShortCodes;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Media
{
    public class MediaShortCodeTests
    {
        [Theory]
        [InlineData("foo bar baz", "foo bar baz")]
        [InlineData("foo [media]bar baz", "foo [media]bar baz")]
        [InlineData("foo [media]bar[/media] baz", @"foo <img src=""/media/bar""> baz")]
        [InlineData("foo [media]bàr.jpeg?width=100[/media] baz", @"foo <img src=""/media/bàr.jpeg?width=100""> baz")]
        [InlineData("foo [media]bàr.jpeg?width=100 onload=\"javascript: alert('XSS')[/media] baz", @"foo <img src=""/media/bàr.jpeg?width=100 onload=""> baz")]

        public async Task ShouldProcess(string text, string expected)
        {
            var mediaShortCode = new MediaShortCode(
            new DefaultMediaFileStore(Mock.Of<IFileStore>(),
                "/media",
                string.Empty,
                Enumerable.Empty<IMediaEventHandler>(),
                Enumerable.Empty<IMediaCreatingEventHandler>(),
                Mock.Of<ILogger<DefaultMediaFileStore>>()),
            new HtmlSanitizerService(Options.Create(new HtmlSanitizerOptions()))
        );

        var processed = await mediaShortCode.ProcessAsync(text);
            Assert.Equal(expected, processed);
        }
    }
}
