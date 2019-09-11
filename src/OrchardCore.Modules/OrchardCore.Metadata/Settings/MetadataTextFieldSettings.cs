using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Metadata.Interfaces;
using OrchardCore.Metadata.Models;

namespace OrchardCore.Metadata.Settings
{
    public class MetadataTextFieldSettings : IMetadataFieldSettings
    {
        public AttributeType DescriptorAttibuteType { get; set; } = AttributeType.name;
        public string Descriptor { get; set; }
        public bool Required { get; set; }
        public string Hint { get; set; }
        public int MaxCharacterLength { get; set; }
        public string DefaultValue { get; set; }
    }
}
