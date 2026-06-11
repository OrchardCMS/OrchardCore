using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Modules;

/// <summary>
/// Provides <see cref="SelectListItem"/> values for rendering a time zone picker.
/// </summary>
public interface ITimeZoneSelectListProvider
{
    /// <summary>
    /// Gets the available time zone items sorted for display.
    /// </summary>
    ValueTask<IReadOnlyList<SelectListItem>> GetTimeZoneSelectListAsync();
}
