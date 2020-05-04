using System.Collections;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Ganss.XSS;
using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Core;
using OrchardCore.Media.Events;
using OrchardCore.Media.SafeCodeFilters;
using Xunit;

namespace OrchardCore.Tests.SafeCodeFilters
{
    public class MediaSafeCodeFilterTests
    {
        private readonly MediaSafeCodeFilter _filter = new MediaSafeCodeFilter(
            new DefaultMediaFileStore(Mock.Of<IFileStore>(),
                "/media",
                string.Empty,
                Enumerable.Empty<IMediaEventHandler>(),
                Enumerable.Empty<IMediaCreatingEventHandler>(),
                Mock.Of<ILogger<DefaultMediaFileStore>>())
        );

        [Theory]
        [InlineData("foo bar baz", "foo bar baz")]
        [InlineData("foo [media]bar baz", "foo [media]bar baz")]
        [InlineData("foo [media]bar[/media] baz", @"foo <img src=""/media/bar""> baz")]
        [InlineData("foo [media]bàr.jpeg?width=100[/media] baz", @"foo <img src=""/media/bàr.jpeg?width=100""> baz")]
        [InlineData("foo [media]bàr.jpeg?width=100 onload=\"javascript: alert('XSS')[/media] baz", @"foo <img src=""/media/bàr.jpeg?width=100 onload=\""javascript: alert(\u0027XSS\u0027)""> baz")]

        public async Task ShouldProcess(string text, string expected)
        {
            var processed = await _filter.ProcessAsync(text);
            Assert.Equal(expected, processed);
        }
    }
}
