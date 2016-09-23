using Orchard.ContentManagement.Metadata.Models;
using System;

namespace Orchard.ContentManagement.MetaData
{
    public class ContentPartInfo
    {
        public string PartName { get; set; }
        public Func<ContentTypePartDefinition, ContentPart> Factory { get; set; }
    }
}
