using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Queries.ViewModels;

public class QueriesIndexViewModel
{
    /// <summary>
    /// The queried definitions on this page, before their row shapes are built.
    /// </summary>
    public IList<Query> Queries { get; set; }
    public ContentOptions Options { get; set; } = new ContentOptions();
    public IShape Pager { get; set; }
    public IEnumerable<string> QuerySourceNames { get; set; }

    /// <summary>
    /// The data context for rendering the queries, the toolbar and the pager in the configured layout.
    /// </summary>
    public AdminListContext List { get; set; }
}

public class QueryEntry
{
    public Query Query { get; set; }
    public bool IsChecked { get; set; }
    public IShape Shape { get; set; }
}

public class ContentOptions
{
    public string Search { get; set; }
    public ContentsBulkAction BulkAction { get; set; }

    #region Lists to populate

    [BindNever]
    public List<SelectListItem> ContentsBulkAction { get; set; }

    #endregion Lists to populate
}

public enum ContentsBulkAction
{
    None,
    Remove,
}
