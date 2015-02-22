using OrchardVNext.Data;
using OrchardVNext.Data.Conventions;

namespace OrchardVNext.Core.Settings.Metadata.Records {
    [Persistent]
    public class ContentPartFieldDefinitionRecord {
        public virtual int Id { get; set; }
        public virtual ContentFieldDefinitionRecord ContentFieldDefinitionRecord { get; set; }
        public virtual string Name { get; set; }
        [StringLengthMax]
        public virtual string Settings { get; set; }
    }
}