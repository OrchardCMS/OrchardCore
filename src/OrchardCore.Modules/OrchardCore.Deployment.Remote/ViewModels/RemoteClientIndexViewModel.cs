using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Deployment.Remote.Models;

namespace OrchardCore.Deployment.Remote.ViewModels;

public class RemoteClientIndexViewModel
{
    public List<RemoteClient> RemoteClients { get; set; }

    public ContentOptions Options { get; set; } = new ContentOptions();

    [BindNever]
    public dynamic Pager { get; set; }
}

public class ContentOptions
{
    public ContentsBulkAction BulkAction { get; set; }

    public string Search { get; set; }

    #region Lists to populate

    [BindNever]
    public List<SelectListItem> ContentsBulkAction { get; set; }

    #endregion Lists to populate
}

public enum ContentsBulkAction
{
    None,
    Remove
}
