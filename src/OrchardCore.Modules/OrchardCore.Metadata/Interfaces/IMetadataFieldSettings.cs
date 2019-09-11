using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Metadata.Models;

namespace OrchardCore.Metadata.Interfaces
{
    public interface IMetadataFieldSettings
    {
        AttributeType DescriptorAttibuteType { get; set; }
        string Descriptor { get; set; }
        bool Required { get; set; }
    }
}
