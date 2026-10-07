namespace OrchardCore.Modules;

/// <summary>
/// Provides key/value pairs for rendering a time zone picker.
/// </summary>
public interface ITimeZoneSelectListProvider
{
    /// <summary>
    /// Gets the available time zone items sorted for display.
    /// </summary>
    ValueTask<IEnumerable<KeyValuePair<string, string>>> GetTimeZoneSelectListAsync(CancellationToken cancellationToken = default);
}
