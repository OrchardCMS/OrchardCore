using OrchardCore.DisplayManagement;

namespace OrchardCore.ContentManagement.Display.Models;

/// <summary>
/// Model for content item summary display with source-generated Arguments provider.
/// </summary>
[GenerateArgumentsProvider]
public partial class ContentItemSummaryModel
{
    public string ContentItemId { get; set; }
    public string DisplayText { get; set; }
    public string ContentType { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public DateTime? ModifiedUtc { get; set; }
    public DateTime? PublishedUtc { get; set; }
    public string Owner { get; set; }
    public string Author { get; set; }
    public bool IsPublished { get; set; }
    public bool HasDraft { get; set; }
}
