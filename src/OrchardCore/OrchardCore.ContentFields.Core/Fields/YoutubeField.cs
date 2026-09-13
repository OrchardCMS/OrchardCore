using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class YoutubeField : ContentField
{
    [Description("The URL used to embed the YouTube video.")]
    public string EmbeddedAddress { get; set; }

    [Description("The original YouTube video URL.")]
    public string RawAddress { get; set; }
}
