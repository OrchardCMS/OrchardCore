using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentManagement;
using OrchardCore.Metadata.Fields;

namespace OrchardCore.Metadata.Models
{
    public class SocialMetadataPart : ContentPart
    {
        public MetadataTextField OpenGraphTitle { get; set; }
        public MetadataTextField OpenGraphDescription { get; set; }
        public string OpenGraphImage { get; set; }
        public string OpenGraphVideo { get; set; }
        public MetadataTextField OpenGraphType { get; set; }
        public MetadataTextField TwitterCard { get; set; }
    }
}
