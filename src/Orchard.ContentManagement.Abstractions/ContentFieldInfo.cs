using Orchard.ContentManagement.Metadata.Models;
using System;

namespace Orchard.ContentManagement.MetaData
{
    public class ContentFieldInfo
    {
        public string FieldTypeName { get; set; }
        public Func<ContentPartFieldDefinition, object> Factory { get; set; }
    }
}
