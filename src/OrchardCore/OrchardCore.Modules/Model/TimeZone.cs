using NodaTime;

namespace OrchardCore.Modules
{
    public class TimeZone : ITimeZone
    {
        public string TimeZoneId { get; set; }
        public Offset Offset { get; set; }

        public TimeZone(string timeZoneId, Offset offset)
        {
            TimeZoneId = timeZoneId;
            Offset = offset;
        }

        public override string ToString()
        {
            return $"({Offset:+HH:mm}) {TimeZoneId}";
        }
    }
}
