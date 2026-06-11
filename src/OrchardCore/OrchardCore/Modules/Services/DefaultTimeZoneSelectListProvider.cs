using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Modules.Services;

public sealed class DefaultTimeZoneSelectListProvider : ITimeZoneSelectListProvider
{
    private readonly IClock _clock;

    private SelectListItem[] _selectListItems;

    public DefaultTimeZoneSelectListProvider(IClock clock)
    {
        _clock = clock;
    }

    /// <inheritdoc/>
    public ValueTask<IReadOnlyList<SelectListItem>> GetTimeZoneSelectListAsync()
    {
        _selectListItems ??= _clock.GetTimeZones()
            .Select(timeZone => new SelectListItem
            {
                Value = timeZone.TimeZoneId,
                Text = timeZone.ToString(),
            })
            .OrderBy(item => item.Text, StringComparer.Ordinal)
            .ToArray();

        return ValueTask.FromResult<IReadOnlyList<SelectListItem>>(_selectListItems);
    }
}
