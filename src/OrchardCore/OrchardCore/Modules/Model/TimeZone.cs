using NodaTime;

namespace OrchardCore.Modules
{
    public class TimeZone : ITimeZone
    {
        public string TimeZoneId { get; set; }

        public Offset StandardOffset { get; set; }

        public Offset WallOffset { get; set; }

        public DateTimeZone DateTimeZone { get; set; }

        public TimeZone(string timeZoneId, Offset standardOffset, Offset wallOffset, DateTimeZone dateTimeZone)
        {
            TimeZoneId = timeZoneId;
            StandardOffset = standardOffset;
            WallOffset = wallOffset;
            DateTimeZone = dateTimeZone;
        }

        public override string ToString()
        {
            return $"(GMT{StandardOffset}) {TimeZoneId} ({WallOffset:+HH:mm})";
        }
    }
}
