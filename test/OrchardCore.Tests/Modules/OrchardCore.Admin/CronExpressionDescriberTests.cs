using System.Globalization;
using OrchardCore.Admin.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public sealed class CronExpressionDescriberTests
{
    private static readonly CultureInfo _english = CultureInfo.GetCultureInfo("en-US");

    [Theory]
    [InlineData("0 3 * * *", "At 03:00 AM")]
    [InlineData("*/5 * * * *", "Every 5 minutes")]
    [InlineData("  0 3 * * *  ", "At 03:00 AM")]
    public void Describe_ValidExpression_ReturnsWhenItOccurs(string expression, string expected)
    {
        // Act
        var description = CronExpressionDescriber.Describe(expression, _english);

        // Assert
        Assert.Equal(expected, description);
    }

    [Fact]
    public void Describe_CultureWithA24HourClock_DoesNotWriteThePeriodOfTheDay()
    {
        // Act
        var description = CronExpressionDescriber.Describe("0 15 * * *", CultureInfo.GetCultureInfo("fr-FR"));

        // Assert
        Assert.Contains("15:00", description, StringComparison.Ordinal);
        Assert.DoesNotContain("PM", description, StringComparison.Ordinal);
        Assert.NotEqual(CronExpressionDescriber.Describe("0 15 * * *", _english), description);
    }

    [Fact]
    public void Describe_CultureWithoutTranslation_ReturnsTheEnglishDescription()
    {
        // Act
        var description = CronExpressionDescriber.Describe("*/5 * * * *", CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("Every 5 minutes", description);
    }

    [Theory]

    // The abbreviation follows daylight saving time.
    [InlineData("America/Los_Angeles", 1, "PST")]
    [InlineData("America/Los_Angeles", 7, "PDT")]
    [InlineData("Asia/Tokyo", 1, "JST")]

    // A time zone without an abbreviation is told by its offset.
    [InlineData("Asia/Kolkata", 1, "IST")]
    [InlineData("America/Sao_Paulo", 1, "UTC-03:00")]
    [InlineData(null, 1, "UTC")]
    public void GetAbbreviation_TimeZone_ReturnsItsAbbreviationAtTheDate(string timeZoneId, int month, string expected)
    {
        // Arrange
        var timeZone = timeZoneId is null ? null : new global::OrchardCore.Modules.Clock().GetTimeZone(timeZoneId);

        // Act
        var abbreviation = CronExpressionDescriber.GetAbbreviation(timeZone, new DateTime(2026, month, 15, 12, 0, 0, DateTimeKind.Utc));

        // Assert
        Assert.Equal(expected, abbreviation);
    }

    [Fact]
    public void Describe_TimeZoneThatIsNotTheOneOfTheSite_TellsItsAbbreviation()
    {
        // Arrange
        var timeZone = new global::OrchardCore.Modules.Clock().GetTimeZone("Asia/Tokyo");
        var now = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.Equal("At 03:00 AM (JST)", CronExpressionDescriber.Describe("0 3 * * *", _english, timeZone, isSiteTimeZone: false, now));
        Assert.Equal("At 03:00 AM", CronExpressionDescriber.Describe("0 3 * * *", _english, timeZone, isSiteTimeZone: true, now));
        Assert.Equal("At 03:00 AM (UTC)", CronExpressionDescriber.Describe("0 3 * * *", _english, timeZone: null, isSiteTimeZone: false, now));
        Assert.Null(CronExpressionDescriber.Describe("not a cron", _english, timeZone, isSiteTimeZone: false, now));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a cron")]
    [InlineData("0 25 * * *")]
    [InlineData("0 3 * *")]

    // The schedules are evaluated with five fields, so an expression with seconds is not one of theirs.
    [InlineData("0 0 3 * * *")]
    public void Describe_InvalidExpression_ReturnsNull(string expression)
    {
        // Act & Assert
        Assert.False(CronExpressionDescriber.IsValid(expression));
        Assert.Null(CronExpressionDescriber.Describe(expression, _english));
        Assert.Empty(CronExpressionDescriber.GetNextOccurrences(expression, new DateTime(2026, 1, 1), 3));
    }

    [Fact]
    public void IsValid_ExpressionLongerThanTheLimit_ReturnsFalse()
    {
        // Arrange
        var expression = "0 3 * * " + string.Join(',', Enumerable.Repeat("1", CronExpressionDescriber.MaxLength));

        // Act & Assert
        Assert.False(CronExpressionDescriber.IsValid(expression));
    }

    [Fact]
    public void GetNextOccurrences_ValidExpression_ReturnsTheOnesThatFollowTheDate()
    {
        // Act
        var occurrences = CronExpressionDescriber.GetNextOccurrences("30 8 * * 1-5", new DateTime(2026, 11, 13, 8, 30, 0), 3);

        // Assert
        Assert.Equal(
            [
                // The date itself is not returned, and the weekend is skipped.
                new DateTime(2026, 11, 16, 8, 30, 0),
                new DateTime(2026, 11, 17, 8, 30, 0),
                new DateTime(2026, 11, 18, 8, 30, 0),
            ],
            occurrences);
    }

    [Fact]
    public void GetNextOccurrences_ExpressionThatNeverOccurs_ReturnsNone()
    {
        // Act
        var occurrences = CronExpressionDescriber.GetNextOccurrences("0 0 31 2 *", new DateTime(2026, 1, 1), 3);

        // Assert
        Assert.True(CronExpressionDescriber.IsValid("0 0 31 2 *"));
        Assert.Empty(occurrences);
    }
}
