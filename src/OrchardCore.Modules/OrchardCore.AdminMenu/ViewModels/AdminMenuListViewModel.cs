using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AdminMenu.ViewModels;

public class AdminMenuListViewModel
{
    public IList<AdminMenuEntry> AdminMenu { get; set; }
    public ContentOptions Options { get; set; } = new ContentOptions();
    public IShape Pager { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the menus, the toolbar and the pager in the configured layout.
    /// </summary>
    public IShape List { get; set; }
}

public class AdminMenuEntry
{
    public Models.AdminMenu AdminMenu { get; set; }
    public bool IsChecked { get; set; }
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
