using OrchardCore.ContentManagement;
using OrchardCore.Metadata.Interfaces;

namespace OrchardCore.Metadata.Fields
{
    public class MetadataTextField : ContentField, IMetadataField
    {
        public string Value { get; set; }
    }
}
