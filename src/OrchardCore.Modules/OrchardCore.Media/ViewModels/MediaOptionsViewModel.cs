namespace OrchardCore.Media.ViewModels;

/// <summary>
/// The media options and the capabilities of the media store, shown on the Media options page.
/// </summary>
public class MediaOptionsViewModel
{
    public MediaOptions Options { get; set; }

    public string StorageName { get; set; }

    public bool HasHierarchicalNamespace { get; set; }

    public bool SupportsAtomicMove { get; set; }

    public bool SignalrEnabled { get; set; }

    public bool TusEnabled { get; set; }
}
