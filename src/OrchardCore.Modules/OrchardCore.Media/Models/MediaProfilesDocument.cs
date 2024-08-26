using OrchardCore.Data.Documents;
using OrchardCore.Modules;

namespace OrchardCore.Media.Models;

public class MediaProfilesDocument : Document
{
    private readonly Dictionary<string, MediaProfile> _mediaProfiles = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, MediaProfile> MediaProfiles
    {
        get => _mediaProfiles;
        set => _mediaProfiles.SetItems(value);
    }
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
