using OrchardCore.Data.Documents;

namespace OrchardCore.Media.Models;

public class MediaProfilesDocument : Document
{
    public Dictionary<string, MediaProfile> MediaProfiles { get; init; } = new Dictionary<string, MediaProfile>(StringComparer.OrdinalIgnoreCase);
}

public class MediaProfile
{
    public string Hint { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Processing.ResizeMode Mode { get; set; }
    public Processing.Format Format { get; set; }
    public int Quality { get; set; }
    public string BackgroundColor { get; set; }
}
