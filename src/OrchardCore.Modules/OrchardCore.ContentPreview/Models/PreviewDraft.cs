using OrchardCore.ContentManagement;

namespace OrchardCore.ContentPreview.Models;

internal sealed class PreviewDraft
{
    public ContentItem ContentItem { get; set; }
    public string PreviewUrl { get; set; }
}
