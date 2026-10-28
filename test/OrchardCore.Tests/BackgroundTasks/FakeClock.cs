using OrchardCore.Modules;

namespace OrchardCore.Tests.BackgroundTasks;

/// <summary>
/// A clock whose current time is set by the test, the time zone conversions being done by the real clock.
/// </summary>
internal sealed class FakeClock : IClock
{
    private readonly Clock _clock = new();

    public FakeClock(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; set; }

    public void Advance(TimeSpan duration) => UtcNow += duration;

    public ITimeZone[] GetTimeZones() => _clock.GetTimeZones();

    public ITimeZone GetTimeZone(string timeZoneId) => _clock.GetTimeZone(timeZoneId);

    public ITimeZone GetSystemTimeZone() => _clock.GetSystemTimeZone();

    public DateTimeOffset ConvertToTimeZone(DateTimeOffset dateTimeOffset, ITimeZone timeZone)
        => _clock.ConvertToTimeZone(dateTimeOffset, timeZone);
}
