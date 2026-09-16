using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Localization;
using OrchardCore.Modules;

namespace OrchardCore.Tests.DisplayManagement;

public class DateTimeShapesTests
{
    private static readonly DateTime _utc = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void TimeSpanShouldRenderTextOnlyByDefault()
    {
        var shapes = CreateShapes();

        var result = Render(shapes.TimeSpan(_utc, _utc.AddDays(3), TimeTag: false));

        Assert.Equal("3 days ago", result);
    }

    [Fact]
    public void TimeSpanShouldRenderTimeTagWhenRequested()
    {
        var shapes = CreateShapes();

        var result = Render(shapes.TimeSpan(_utc, _utc.AddDays(3), TimeTag: true));

        Assert.Equal("<time datetime=\"2026-01-01T10:00:00Z\">3 days ago</time>", result);
    }

    [Fact]
    public async Task DateTimeShouldRenderTimeTagWithLocalOffsetWhenRequested()
    {
        var shapes = CreateShapes(TimeSpan.FromHours(2));

        var result = Render(await shapes.DateTime(null, _utc, "yyyy-MM-dd", TimeTag: true));

        // The HTML encoder writes '+' as '&#x2B;' in attribute values; browsers decode it back.
        Assert.Equal("<time datetime=\"2026-01-01T12:00:00&#x2B;02:00\">2026-01-01</time>", result);
    }

    [Fact]
    public async Task DateTimeShouldEncodeTextInsideTimeTag()
    {
        var shapes = CreateShapes();

        var result = Render(await shapes.DateTime(null, _utc, "'<b>'yyyy", TimeTag: true));

        Assert.Equal("<time datetime=\"2026-01-01T10:00:00&#x2B;00:00\">&lt;b&gt;2026</time>", result);
    }

    private static DateTimeShapes CreateShapes(TimeSpan offset = default)
    {
        var htmlLocalizer = new Mock<IHtmlLocalizer<DateTimeShapes>>();
        htmlLocalizer
            .Setup(localizer => localizer[It.IsAny<string>()])
            .Returns((string name) => NullHtmlLocalizer.Instance[name]);
        htmlLocalizer
            .Setup(localizer => localizer[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => NullHtmlLocalizer.Instance[name, arguments]);

        var localClock = new Mock<ILocalClock>();
        localClock
            .Setup(clock => clock.ConvertToLocalAsync(It.IsAny<DateTimeOffset>()))
            .ReturnsAsync((DateTimeOffset dateTime) => dateTime.ToOffset(offset));

        return new DateTimeShapes(
            Mock.Of<IClock>(),
            Mock.Of<IStringLocalizer<DateTimeShapes>>(),
            htmlLocalizer.Object,
            localClock.Object);
    }

    private static string Render(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }
}
