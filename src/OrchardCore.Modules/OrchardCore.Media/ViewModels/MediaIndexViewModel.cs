namespace OrchardCore.Media.ViewModels;

public sealed class MediaIndexViewModel
{
    public string SiteId { get; set; } = string.Empty;

    public long MaxFileSize { get; set; }

    public string AllowedExtensions { get; set; } = string.Empty;

    public bool TusEnabled { get; set; }

    public bool SignalrEnabled { get; set; }

    public bool DebugEnabled { get; set; }
}
