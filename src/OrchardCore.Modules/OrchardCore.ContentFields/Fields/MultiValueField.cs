using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields
{
    public class MultiValueField : ContentField
    {
        public string[] Values { get; set; }
    }
}
