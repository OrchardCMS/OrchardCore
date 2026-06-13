namespace OrchardCore.ContentPreview;

/// <summary>
/// Used when a content item is being previewed.
/// </summary>
public class ContentPreviewFeature
{
    public static readonly ContentPreviewFeature Instance = new();

    public bool Previewing { get; set; } = true;
}
