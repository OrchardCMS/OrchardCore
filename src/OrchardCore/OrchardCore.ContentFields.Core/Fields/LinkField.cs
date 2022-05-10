using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields
{
    public class LinkField : ContentField
    {
        public string Url { get; set; }

        public string Text { get; set; }
    }
}
