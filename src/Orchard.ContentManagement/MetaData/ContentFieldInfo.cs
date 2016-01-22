using Orchard.ContentManagement.MetaData.Models;
using System;

namespace Orchard.ContentManagement.MetaData
{
    public class ContentFieldInfo
    {
        public string FieldName { get; set; }
        public Func<ContentPartFieldDefinition, object> Factory { get; set; }
    }
}
