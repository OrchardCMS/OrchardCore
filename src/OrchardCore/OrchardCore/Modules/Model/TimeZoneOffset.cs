using NodaTime;

namespace OrchardCore.Modules
{
    public class TimeZoneOffset : ITimeZoneOffset
    {
        public long Offset { get; set; }

        public Offset StandardOffset { get; set; }

        public Offset WallOffset { get; set; }

        public TimeZoneOffset(Offset standardOffset, Offset wallOffset)
        {
            StandardOffset = standardOffset;
            WallOffset = wallOffset;
            Offset = standardOffset.Seconds;
        }

        public override string ToString()
        {
            return $"(GMT{StandardOffset}) {WallOffset:+HH:mm}";
        }
    }
}
