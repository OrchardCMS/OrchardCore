using OrchardVNext.ContentManagement.FieldStorage;
using OrchardVNext.ContentManagement.MetaData.Models;

namespace OrchardVNext.ContentManagement {
    public class ContentField {
        public string Name { get { return PartFieldDefinition.Name; } }
        public string DisplayName { get { return PartFieldDefinition.DisplayName; } }

        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
        public ContentFieldDefinition FieldDefinition { get { return PartFieldDefinition.FieldDefinition; } }

        public IFieldStorage Storage { get; set; }
    }
}
