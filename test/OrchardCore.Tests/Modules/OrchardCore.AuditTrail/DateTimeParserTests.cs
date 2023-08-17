using OrchardCore.AuditTrail.Services;
using OrchardCore.Modules;
using Parlot;

namespace OrchardCore.Tests.Modules.OrchardCore.AuditTrail
{
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
        public void DateParserTests(string text, string expected)
        {
            var context = new DateTimeParseContext(CultureInfo.InvariantCulture, Mock.Of<IClock>(), Mock.Of<ITimeZone>(), new Scanner(text));
            Assert.True(DateTimeParser.Parser.TryParse(context, out var result, out _));
            Assert.StartsWith(expected, result.ToString());
        }
    }
}
