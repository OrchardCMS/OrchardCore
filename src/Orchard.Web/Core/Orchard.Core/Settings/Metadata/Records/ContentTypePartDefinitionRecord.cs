using Orchard.Data.Conventions;

namespace Orchard.Core.Settings.Metadata.Records {
    public class ContentTypePartDefinitionRecord {
        public virtual int Id { get; set; }
        public virtual ContentPartDefinitionRecord ContentPartDefinitionRecord { get; set; }
        [StringLengthMax]
        public virtual string Settings { get; set; }
    }
}
