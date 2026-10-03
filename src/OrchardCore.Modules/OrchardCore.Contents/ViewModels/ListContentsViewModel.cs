using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Contents.ViewModels;

public class ListContentsViewModel
{
    public ContentOptionsViewModel Options { get; set; }

    [BindNever]
    public IShape Header { get; set; }

    [BindNever]
    public List<dynamic> ContentItems { get; set; }

    [BindNever]
    public IShape Pager { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering <see cref="ContentItems"/>, <see cref="Header"/> and <see cref="Pager"/> with the configured layout.
    /// </summary>
    [BindNever]
    public IShape List { get; set; }
}
