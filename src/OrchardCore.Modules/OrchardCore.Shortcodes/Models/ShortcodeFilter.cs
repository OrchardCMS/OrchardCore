using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Shortcodes.ViewModels;
using YesSql.Filters.Enumerable;

namespace OrchardCore.Shortcodes.Services;

/// <summary>
/// Shortcode filters.
/// </summary>
public class ShortcodeFilter
{
    public string SearchText { get; set; }

    public string OriginalSearchText { get; set; }

    public string Name { get; set; }

    public ContentsOrder OrderBy { get; set; }

    public ContentsBulkAction BulkAction { get; set; }

    [ModelBinder(BinderType = typeof(ShortcodeFilterEngineModelBinder), Name = nameof(SearchText))]
    public EnumerableFilterResult<DataSourceEntry> FilterResult { get; set; }

    #region Values to populate

    /// <summary>
    /// This gets passed through so we can count statuses.
    /// </summary>
    [BindNever]
    public IReadOnlyList<DataSourceEntry> AllItems { get; set; }

    [BindNever]
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }

    [BindNever]
    public int ShortcodeItemsCount { get; set; }

    [BindNever]
    public int TotalItemCount { get; set; }

    [BindNever]
    public RouteValueDictionary RouteValues { get; set; } = [];

    #endregion

    #region Lists to populate

    [BindNever]
    public List<SelectListItem> ContentsBulkAction { get; set; }

    [BindNever]
    public List<SelectListItem> ContentSorts { get; set; }

    #endregion Lists to populate
}
