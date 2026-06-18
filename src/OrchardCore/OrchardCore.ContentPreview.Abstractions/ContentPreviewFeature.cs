namespace OrchardCore.ContentPreview;

/// <summary>
/// Used when a content item is being previewed.
/// </summary>
public sealed class ContentPreviewFeature
{
    public static readonly ContentPreviewFeature Instance = new();

    public bool Previewing { get; init; } = true;
}
