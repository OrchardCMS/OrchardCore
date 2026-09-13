using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Contents.ViewModels;

public class ListContentsViewModel
{
    public ContentOptionsViewModel Options { get; set; }

    [BindNever]
    public dynamic Header { get; set; }

    [BindNever]
    public List<dynamic> ContentItems { get; set; }

    [BindNever]
    public dynamic Pager { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering <see cref="ContentItems"/>, <see cref="Header"/> and <see cref="Pager"/> with the configured layout.
    /// </summary>
    [BindNever]
    public dynamic List { get; set; }
}
