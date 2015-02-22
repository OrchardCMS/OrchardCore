using OrchardVNext.Data;

namespace OrchardVNext.Core.Settings.Metadata.Records {
    [Persistent]
    public class ContentFieldDefinitionRecord {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
    }
}
