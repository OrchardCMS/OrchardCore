using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.MetaData.Models
{
    public class ContentDefinition
    {
        public ContentDefinition()
        {
            ContentTypeDefinitions = new Dictionary<string, ContentTypeDefinition>();
            ContentPartDefinitions = new Dictionary<string, ContentPartDefinition>();
            ContentFieldDefinitions = new Dictionary<string, ContentFieldDefinition>();
        }
        public IDictionary<string, ContentTypeDefinition> ContentTypeDefinitions { get; set; }
        public IDictionary<string, ContentPartDefinition> ContentPartDefinitions { get; set; }
        public IDictionary<string, ContentFieldDefinition> ContentFieldDefinitions { get; set; }
    }
}
