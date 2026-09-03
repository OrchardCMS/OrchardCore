namespace OrchardCore.Navigation;

public class PagerOptions
{
    private const int DefaultPageSize = 10;

    public int PageSize { get; set; } = DefaultPageSize;

    public int MaxPageSize { get; set; } = 100;

    public int MaxPagedCount { get; set; }

    /// <summary>
    /// Gets or sets whether users are allowed to change the number of items displayed per page
    /// by selecting one of the values defined in <see cref="PageSizeOptions"/>.
    /// </summary>
    public bool AllowPageSizeSelection { get; set; }

    /// <summary>
    /// Gets or sets the page size values a user is allowed to select from when
    /// <see cref="AllowPageSizeSelection"/> is enabled.
    /// </summary>
    public int[] PageSizeOptions { get; set; }

    public int GetPageSize()
    {
        if (MaxPageSize > 0 && PageSize > MaxPageSize)
        {
            return MaxPageSize;
        }

        return PageSize > 0 ? PageSize : DefaultPageSize;
    }

    /// <summary>
    /// Resolves the effective page size for a request, honoring the user selected value only when
    /// page size selection is enabled and the requested value is one of the configured
    /// <see cref="PageSizeOptions"/>. Any other value falls back to the configured default.
    /// </summary>
    /// <param name="selectedPageSize">The page size requested for the current listing, or <c>null</c> when none was provided.</param>
    /// <returns>The page size to use.</returns>
    public int GetPageSize(int? selectedPageSize)
    {
        if (AllowPageSizeSelection &&
            selectedPageSize.HasValue &&
            PageSizeOptions is { Length: > 0 } &&
            Array.IndexOf(PageSizeOptions, selectedPageSize.Value) >= 0)
        {
            if (MaxPageSize > 0 && selectedPageSize.Value > MaxPageSize)
            {
                return MaxPageSize;
            }

            if (selectedPageSize.Value > 0)
            {
                return selectedPageSize.Value;
            }
        }

        return GetPageSize();
    }
}
