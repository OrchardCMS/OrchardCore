using OrchardCore.AuditTrail.Services;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Modules.OrchardCore.AuditTrail;

public class DateTimeRangeTests
{
    [Theory]
    [InlineData("@now", "@now")]
    [InlineData("@now-1", "@now-1")]
    [InlineData("@now-2..@now-1", "@now-2..@now-1")]
    [InlineData("@now+2", "@now2")]
    [InlineData(">@now", ">@now")]
    [InlineData(">=@now", ">=@now")]
    //[InlineData("2019-10-12", "2019-10-12T00:00:00.0000000")]
    //[InlineData(">2019-10-12", ">2019-10-12T00:00:00.0000000")]
    //[InlineData("2017-01-01T01:00:00+07:00", "2017-01-01T01:00:00.0000000+07:00")]
    //[InlineData("2017-01-01T01:00:00+07:00..2017-01-01T01:00:00+07:00", "2017-01-01T01:00:00.0000000+07:00..2017-01-01T01:00:00.0000000+07:00")]
    public void DateParser_Default_Succeeds(string text, string expected)
    {
        Assert.True(DateTimeParser.Parser.TryParse(CreateContext(text), out var result, out _));
        Assert.StartsWith(expected, result.ToString());
    }

    [Theory]
    [InlineData("not a date")]
    [InlineData(">not a date")]
    [InlineData("@now..not a date")]
    [InlineData("not a date..@now")]
    public void DateParser_InvalidDate_Fails(string text)
    {
        Assert.False(DateTimeParser.Parser.TryParse(CreateContext(text), out _, out _));
    }

    [Theory]
    [InlineData("@now..", "@now")]
    [InlineData("@today..", "@today")]
    [InlineData("<@today-3", "<@today-3")]
    public void DateParser_NoEndOfRange_ReturnsStart(string text, string expected)
    {
        Assert.True(DateTimeParser.Parser.TryParse(CreateContext(text), out var result, out _));
        Assert.IsType<UnaryExpressionNode>(result);
        Assert.Equal(expected, result.ToString());
    }

    [Fact]
    public void DateParser_DateWithOffset_ConvertsToUtc()
    {
        Assert.True(DateTimeParser.Parser.TryParse(CreateContext(">2019-10-12T01:00:00+02:00"), out var result, out _));
        Assert.Contains("2019-10-11T23:00:00", result.ToString());
    }

    private static DateTimeParseContext CreateContext(string text)
        => new(CultureInfo.InvariantCulture, Mock.Of<IClock>(), Mock.Of<ITimeZone>(), new Scanner(text));
}
