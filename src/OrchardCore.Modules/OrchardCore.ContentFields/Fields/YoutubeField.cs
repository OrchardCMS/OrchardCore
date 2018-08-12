using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields
{
    public class YoutubeField : ContentField
    {
        public string EmbeddedAddress { get; set; }
        public string RawAddress { get; set; }
    }
}
