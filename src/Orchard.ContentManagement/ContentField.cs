using Orchard.ContentManagement.MetaData.Models;
using System.Runtime.Serialization;

namespace Orchard.ContentManagement
{
    public class ContentField
    {
        public string Name { get { return PartFieldDefinition.Name; } }
        public string DisplayName { get { return PartFieldDefinition.DisplayName; } }

        [IgnoreDataMember]
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
        [IgnoreDataMember]
        public ContentFieldDefinition FieldDefinition { get { return PartFieldDefinition.FieldDefinition; } }
    }
}