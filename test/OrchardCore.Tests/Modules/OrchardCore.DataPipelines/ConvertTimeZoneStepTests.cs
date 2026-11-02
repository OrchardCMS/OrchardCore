using OrchardCore.DataPipelines;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;
using OrchardCore.Modules;
using OrchardCore.Tests.Modules.OrchardCore.DataSources;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class ConvertTimeZoneStepTests
{
    private static readonly DataField[] _fields =
    [
        new("Id", "Id", DataFieldType.Integer),
        new("Created", "Created", DataFieldType.DateTime),
        new("Shipped", "Shipped", DataFieldType.DateTime),
        new("Day", "Day", DataFieldType.Date),
    ];

    private static readonly Clock _clock = new();

    [Fact]
    public async Task Execute_FromUtcToSiteTimeZone_FollowsDaylightSavingTime()
    {
        // Arrange
        var settings = new ConvertTimeZoneStepSettings { Fields = ["Created"] };
        using var host = new StepTestHost(CreateStep(), settings).WithRows(
            _fields,
            [1L, Utc(2026, 1, 5, 15, 0), Utc(2026, 1, 5, 15, 0), new DateTime(2026, 1, 5)],
            [2L, Utc(2026, 7, 1, 15, 0), null, null]);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var output = host.Output();

        // The site is in New York: UTC-5 in winter, and UTC-4 in summer.
        Assert.Equal(new DateTime(2026, 1, 5, 10, 0, 0), output.Value(0, "Created"));
        Assert.Equal(new DateTime(2026, 7, 1, 11, 0, 0), output.Value(1, "Created"));

        // A local time has no kind, so it is written without "Z".
        Assert.Equal(DateTimeKind.Unspecified, Assert.IsType<DateTime>(output.Value(0, "Created")).Kind);

        // The fields that aren't selected keep their values.
        Assert.Equal(Utc(2026, 1, 5, 15, 0), output.Value(0, "Shipped"));
        Assert.Null(output.Value(1, "Shipped"));
    }

    [Fact]
    public async Task Execute_NoFieldSelected_ConvertsEveryDateAndTimeButNotDates()
    {
        // Arrange
        var settings = new ConvertTimeZoneStepSettings { TimeZone = TimeZoneSource.Specific, TimeZoneId = "Asia/Tokyo" };
        using var host = new StepTestHost(CreateStep(), settings).WithRows(
            _fields,
            [1L, Utc(2026, 1, 5, 15, 0), Utc(2026, 1, 5, 20, 0), new DateTime(2026, 1, 5)]);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var output = host.Output();
        Assert.Equal(new DateTime(2026, 1, 6, 0, 0, 0), output.Value(0, "Created"));
        Assert.Equal(new DateTime(2026, 1, 6, 5, 0, 0), output.Value(0, "Shipped"));
        Assert.Equal(new DateTime(2026, 1, 5), output.Value(0, "Day"));
        Assert.Equal(_fields, output.RuntimeFields);
    }

    [Theory]
    [InlineData(2026, 7, 1, 14, 0, 2026, 7, 1, 12, 0)]
    [InlineData(2026, 1, 5, 14, 0, 2026, 1, 5, 13, 0)]

    // 02:30 doesn't exist when Paris moves to summer time: it takes the offset before the change, UTC+1.
    [InlineData(2026, 3, 29, 2, 30, 2026, 3, 29, 1, 30)]

    // 02:30 happens twice when Paris moves back to winter time: the first one, in summer time, is taken.
    [InlineData(2026, 10, 25, 2, 30, 2026, 10, 25, 0, 30)]
    public async Task Execute_ToUtcFromSpecificTimeZone_ConvertsLocalTimes(int year, int month, int day, int hour, int minute, int utcYear, int utcMonth, int utcDay, int utcHour, int utcMinute)
    {
        // Arrange
        var settings = new ConvertTimeZoneStepSettings
        {
            Fields = ["Created"],
            Conversion = TimeZoneConversion.ToUtc,
            TimeZone = TimeZoneSource.Specific,
            TimeZoneId = "Europe/Paris",
        };
        using var host = new StepTestHost(CreateStep(), settings).WithRows(
            _fields,
            [1L, new DateTime(year, month, day, hour, minute, 0), null, null]);

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var converted = Assert.IsType<DateTime>(host.Output().Value(0, "Created"));
        Assert.Equal(Utc(utcYear, utcMonth, utcDay, utcHour, utcMinute), converted);
        Assert.Equal(DateTimeKind.Utc, converted.Kind);
    }

    [Fact]
    public async Task Execute_UserTimeZone_UsesTheTimeZoneOfTheRunUser()
    {
        // Arrange
        var settings = new ConvertTimeZoneStepSettings { Fields = ["Created"], TimeZone = TimeZoneSource.User };
        using var host = new StepTestHost(CreateStep(), settings).WithRows(
            _fields,
            [1L, Utc(2026, 1, 5, 15, 0), null, null]);
        host.Run.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "ada")], "Test"));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(new DateTime(2026, 1, 5, 16, 0, 0), host.Output().Value(0, "Created"));
    }

    [Theory]
    [InlineData("Id", "isn't a date and time")]
    [InlineData("Day", "isn't a date and time")]
    [InlineData("Missing", "no field named")]
    public async Task Describe_FieldThatCantBeConverted_ReportsError(string field, string expected)
    {
        // Arrange
        var settings = new ConvertTimeZoneStepSettings { Fields = [field] };
        using var host = new StepTestHost(CreateStep(), settings).WithRows(_fields);

        // Act
        var context = await host.DescribeAsync();

        // Assert
        var issue = Assert.Single(context.Issues);
        Assert.Equal(DataPipelineIssueSeverity.Error, issue.Severity);
        Assert.Contains(expected, issue.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null, "Choose a time zone")]
    [InlineData("Mars/Olympus_Mons", "no time zone named")]
    public async Task Describe_SpecificTimeZoneThatDoesNotExist_ReportsError(string timeZoneId, string expected)
    {
        // Arrange
        var settings = new ConvertTimeZoneStepSettings { TimeZone = TimeZoneSource.Specific, TimeZoneId = timeZoneId };
        using var host = new StepTestHost(CreateStep(), settings).WithRows(_fields);

        // Act
        var context = await host.DescribeAsync();

        // Assert
        var issue = Assert.Single(context.Issues);
        Assert.Contains(expected, issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ToUtc_LocalTimeConvertedFromUtc_ReturnsTheSameInstant()
    {
        // Arrange
        var step = CreateStep();
        var timeZone = _clock.GetTimeZone("America/Los_Angeles");

        for (var utc = Utc(2026, 1, 1, 0, 0); utc < Utc(2027, 1, 1, 0, 0); utc = utc.AddHours(7))
        {
            // Act
            var local = step.FromUtc(utc, timeZone);
            var back = step.ToUtc(local, timeZone);

            // Assert
            // A local time that is repeated maps back to its first occurrence.
            Assert.True(back == utc || back == utc.AddHours(-1), $"{utc:o} became {local:o}, then {back:o}.");
        }
    }

    private static ConvertTimeZoneStep CreateStep()
    {
        return new ConvertTimeZoneStep(_clock, new TestTimeZoneResolver(), new PassThroughStringLocalizer<ConvertTimeZoneStep>());
    }

    private static DateTime Utc(int year, int month, int day, int hour, int minute)
    {
        return new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
    }

    private sealed class TestTimeZoneResolver : IDataTimeZoneResolver
    {
        public Task<ITimeZone> GetSiteTimeZoneAsync()
        {
            return Task.FromResult(_clock.GetTimeZone("America/New_York"));
        }

        public Task<ITimeZone> GetUserTimeZoneAsync(ClaimsPrincipal user)
        {
            return user?.Identity?.Name == "ada"
                ? Task.FromResult(_clock.GetTimeZone("Europe/Paris"))
                : GetSiteTimeZoneAsync();
        }

        public ITimeZone GetTimeZone(string timeZoneId)
        {
            if (string.IsNullOrEmpty(timeZoneId))
            {
                return null;
            }

            var timeZone = _clock.GetTimeZone(timeZoneId);

            return timeZone.TimeZoneId == timeZoneId ? timeZone : null;
        }
    }
}
