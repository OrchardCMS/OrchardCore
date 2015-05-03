using OrchardVNext.Data;
using OrchardVNext.Data.Conventions;

namespace OrchardVNext.Core.Settings.Metadata.Records {
    [Persistent]
    public class ContentTypePartDefinitionRecord {
        public virtual int Id { get; set; }
        public virtual ContentPartDefinitionRecord ContentPartDefinitionRecord { get; set; }
        [StringLengthMax]
        public virtual string Settings { get; set; }
    }
}
