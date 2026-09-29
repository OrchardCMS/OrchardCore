using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;
using OrchardCore.Media.Models;

namespace OrchardCore.Media.ViewModels;

public class MediaProfileIndexViewModel
{
    public IList<MediaProfileEntry> MediaProfiles { get; set; }
    public IShape Pager { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the profiles, the toolbar and the pager in the configured layout.
    /// </summary>
    public IShape List { get; set; }
    public ContentOptions Options { get; set; } = new ContentOptions();
}

public class MediaProfileEntry
{
    public string Name { get; set; }
    public MediaProfile MediaProfile { get; set; }
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
