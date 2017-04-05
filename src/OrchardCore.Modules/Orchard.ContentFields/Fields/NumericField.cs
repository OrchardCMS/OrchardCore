using Orchard.ContentManagement;

namespace Orchard.ContentFields.Fields
{
    public class NumericField : ContentField
    {
        public decimal? Value { get; set; }
    }
}
