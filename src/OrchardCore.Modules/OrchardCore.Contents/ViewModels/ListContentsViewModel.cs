using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Contents.ViewModels;

public class ListContentsViewModel
{
    public ContentOptionsViewModel Options { get; set; }

    [BindNever]
    public IShape Header { get; set; }

    /// <summary>
    /// The queried content items on this page, before their row shapes are built.
    /// </summary>
    [BindNever]
    public List<ContentItem> ContentItems { get; set; }

    [BindNever]
    public IShape Pager { get; set; }

    /// <summary>
    /// The data context for rendering <see cref="ContentItems"/>, <see cref="Header"/> and <see cref="Pager"/> with the configured layout.
    /// </summary>
    [BindNever]
    public AdminListContext List { get; set; }
}
