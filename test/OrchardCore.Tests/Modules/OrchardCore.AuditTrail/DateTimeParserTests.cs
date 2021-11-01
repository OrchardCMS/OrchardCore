using System;
using System.Globalization;
using Moq;
using NodaTime.TimeZones;
using OrchardCore.AuditTrail.Services;
using OrchardCore.Modules;
using Parlot;
using Xunit;

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
        [InlineData("2019-10-12", "2019-10-12T00:00:00.0000000")]
        [InlineData(">2019-10-12", ">2019-10-12T00:00:00.0000000")]
        [InlineData("2017-01-01T01:00:00+07:00", "2017-01-01T01:00:00.0000000+07:00")]
        [InlineData("2017-01-01T01:00:00+07:00..2017-01-01T01:00:00+07:00", "2017-01-01T01:00:00.0000000+07:00..2017-01-01T01:00:00.0000000+07:00")]
        public void DateParserTests(string text, string expected)
        {
            // Arrange
            var timeZoneMock = new Mock<ITimeZone>();
            var clockMock = new Mock<IClock>();

            timeZoneMock
                .Setup(t => t.TimeZoneId)
                .Returns(TimeZoneInfo.Local.Id);

            clockMock
                .Setup(c => c.ConvertToTimeZone(It.IsAny<DateTimeOffset>(), It.IsAny<ITimeZone>()))
                .Returns<DateTimeOffset, ITimeZone>((d, z) =>
                {
                    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(z.TimeZoneId);
                    var timeZone = BclDateTimeZone.FromTimeZoneInfo(timeZoneInfo);

                    return NodaTime.OffsetDateTime
                        .FromDateTimeOffset(d)
                        .InZone(timeZone)
                        .ToDateTimeOffset();
                });

            // Act
            var context = new DateTimeParseContext(CultureInfo.InvariantCulture, clockMock.Object, timeZoneMock.Object, new Scanner(text));

            // Assert
            Assert.True(DateTimeParser.Parser.TryParse(context, out var result, out var error));
            Assert.StartsWith(expected, result.ToString());
        }
    }
}
