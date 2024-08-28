using OrchardCore.Data.Documents;
using Format = OrchardCore.Media.Processing.Format;
using ResizeMode = OrchardCore.Media.Processing.ResizeMode;

namespace OrchardCore.Media.Models;

public class MediaProfilesDocument : Document
{
    public Dictionary<string, MediaProfile> MediaProfiles { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public class MediaProfile
{
    public string Hint { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public ResizeMode Mode { get; set; }
    public Format Format { get; set; }
    public int Quality { get; set; }
    public string BackgroundColor { get; set; }
}
