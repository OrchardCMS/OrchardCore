using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Modules.Services;

public sealed class DefaultTimeZoneSelectListProvider : ITimeZoneSelectListProvider
{
    private readonly IClock _clock;

    public DefaultTimeZoneSelectListProvider(IClock clock)
    {
        _clock = clock;
    }

    /// <inheritdoc/>
    public IReadOnlyList<SelectListItem> GetTimeZoneSelectListItems()
        => [.. _clock.GetTimeZones()
            .Select(timeZone => new SelectListItem
            {
                Value = timeZone.TimeZoneId,
                Text = timeZone.ToString(),
            })
            .OrderBy(item => item.Text, StringComparer.Ordinal)];
}
