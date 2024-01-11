using OrchardCore.ContentManagement;

namespace OrchardCore.Media.Fields
{
    public class MediaField : ContentField
    {
        public string[] Paths { get; set; }
        public string[] MediaTexts { get; set; }
    }
}
