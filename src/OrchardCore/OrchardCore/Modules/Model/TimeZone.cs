using NodaTime;

namespace OrchardCore.Modules
{
    public class TimeZone : ITimeZone
    {
        public string TimeZoneId { get; set; }

        public Offset Offset { get; set; }

        public Offset DSTOffset { get; set; }

        public DateTimeZone DateTimeZone { get; set; }

        public TimeZone(string timeZoneId, Offset offset, Offset dstOffset, DateTimeZone dateTimeZone)
        {
            TimeZoneId = timeZoneId;
            Offset = offset;
            DSTOffset = dstOffset;
            DateTimeZone = dateTimeZone;
        }

        public override string ToString()
        {
            return $"(GMT{Offset}) {TimeZoneId} ({DSTOffset:+HH:mm})";
        }
    }
}
