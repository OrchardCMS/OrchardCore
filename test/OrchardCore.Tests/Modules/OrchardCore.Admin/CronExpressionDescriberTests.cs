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
