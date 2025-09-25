using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using YesSql.Filters.Query;

namespace OrchardCore.Shortcodes.ViewModels;

public class ContentOptionsViewModel
{
    public string SearchText { get; set; }
    public string OriginalSearchText { get; set; }
    public ContentsBulkAction BulkAction { get; set; }
    public ContentsOrder OrderBy { get; set; }
    public QueryFilterResult<ShortcodeTemplateEntry> FilterResult { get; set; }

    #region Values to populate

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

public enum ContentsBulkAction
{
    None,
    Remove,
}

public enum ContentsOrder
{
    Modified,
    Created,
    Title,
}
