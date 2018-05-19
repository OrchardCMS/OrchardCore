using NodaTime;

namespace OrchardCore.Modules
{
    public class TimeZone : ITimeZone
    {
        public string TimeZoneId { get; set; }
        public Offset Offset { get; set; }
        public DateTimeZone DateTimeZone { get;set;}

        public TimeZone(string timeZoneId, Offset offset, DateTimeZone dateTimeZone)
        {
            TimeZoneId = timeZoneId;
            Offset = offset;
            DateTimeZone = dateTimeZone;
        }

        public override string ToString()
        {
            return $"({Offset:+HH:mm}) {TimeZoneId}";
        }
    }
}
