using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Contents.ViewModels;

/// <summary>
/// Holds the independent content editors displayed in the batch creation form.
/// </summary>
public sealed class BatchCreateContentViewModel
{
    public string ContentType { get; set; }

    public string ContentTypeDisplayName { get; set; }

    public int Count { get; set; }

    public List<ContentItem> ContentItems { get; } = [];

    public List<IShape> Editors { get; } = [];
}
