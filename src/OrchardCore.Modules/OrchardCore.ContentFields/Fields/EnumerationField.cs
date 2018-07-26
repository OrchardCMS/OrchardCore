using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields
{
    public class EnumerationField : ContentField
    {
        public string Value { get; set; }

        public string[] SelectedValues{ get; set; }
    }
}
