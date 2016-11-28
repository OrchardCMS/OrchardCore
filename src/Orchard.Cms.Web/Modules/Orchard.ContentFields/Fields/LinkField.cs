using Orchard.ContentManagement;

namespace Orchard.ContentFields.Fields
{
    public class LinkField : ContentField
    {
        public string Value { get; set; }

        public string Text { get; set; }

        public string Target { get; set; }
    }
}
