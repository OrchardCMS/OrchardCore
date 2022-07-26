using OrchardCore.ContentManagement;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Fields
{
    [Storable(typeof(TextField))]
    public class TextField : ContentField
    {
        public string Text { get; set; }
    }
}
