using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentFields.Settings;

public class YoutubeFieldSettings : FieldSettings
{
    public string Label { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }
}
