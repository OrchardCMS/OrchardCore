namespace OrchardCore.Navigation;

/// <summary>
/// Represents the paging parameters of a safe navigation that doesn't
/// require counting.
/// </summary>
public class PagerSlimParameters
{
    /// <summary>
    /// Gets or sets the first item displayed on the page.
    /// </summary>
    public string Before { get; set; }

    /// <summary>
    /// Gets or sets the last item displayed on the page.
    /// </summary>
    public string After { get; set; }

    /// <summary>
    /// Gets or sets the requested page size, or <c>null</c> when none is specified.
    /// Only honored when the site allows page size selection and the value is one of the configured options.
    /// </summary>
    public int? PageSize { get; set; }
}
