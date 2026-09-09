using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

using OrchardCore.Placements.Models;

namespace OrchardCore.Placements.ViewModels;

public class ListShapePlacementsViewModel
{
    public IList<ShapePlacement> ShapePlacements { get; set; }
    public dynamic Pager { get; set; }
    public ContentOptions Options { get; set; } = new ContentOptions();

    /// <summary>
    /// The <c>AdminList</c> shape rendering the placements, the toolbar and the pager in the configured layout.
    /// </summary>
    public dynamic List { get; set; }
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
