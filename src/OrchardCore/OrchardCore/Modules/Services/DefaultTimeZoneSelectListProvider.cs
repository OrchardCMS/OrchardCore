namespace OrchardCore.Modules.Services;

public sealed class DefaultTimeZoneSelectListProvider : ITimeZoneSelectListProvider
{
    private readonly IClock _clock;

    private KeyValuePair<string, string>[] _items;

    public DefaultTimeZoneSelectListProvider(IClock clock)
    {
        _clock = clock;
    }

    /// <inheritdoc/>
    public ValueTask<IEnumerable<KeyValuePair<string, string>>> GetTimeZoneSelectListAsync(CancellationToken cancellationToken = default)
    {
        _items ??= _clock.GetTimeZones()
            .Select(timeZone => new KeyValuePair<string, string>(timeZone.TimeZoneId, timeZone.ToString()))
            .OrderBy(item => item.Value, StringComparer.Ordinal)
            .ToArray();

        return ValueTask.FromResult<IEnumerable<KeyValuePair<string, string>>>(_items);
    }
}
