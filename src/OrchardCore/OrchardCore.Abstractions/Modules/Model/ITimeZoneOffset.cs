namespace OrchardCore.Modules
{
    /// <summary>
    /// Represents a time zone.
    /// </summary>
    public interface ITimeZoneOffset
    {
        public long Offset { get; set; }
    }
}
