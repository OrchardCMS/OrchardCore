namespace OrchardCore.Modules;

public class TimeZoneSelectorResult
{
    public int Priority { get; set; }
    public Func<Task<string>> TimeZoneId { get; set; }
}
