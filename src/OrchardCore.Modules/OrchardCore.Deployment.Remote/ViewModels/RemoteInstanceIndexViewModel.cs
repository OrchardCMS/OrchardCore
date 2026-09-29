using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Deployment.Remote.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Deployment.Remote.ViewModels;

public class RemoteInstanceIndexViewModel
{
    public List<RemoteInstance> RemoteInstances { get; set; }

    public ContentOptions Options { get; set; } = new ContentOptions();

    [BindNever]
    public IShape Pager { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the rows, the toolbar and the pager in the configured layout.
    /// </summary>
    [BindNever]
    public IShape List { get; set; }
}
