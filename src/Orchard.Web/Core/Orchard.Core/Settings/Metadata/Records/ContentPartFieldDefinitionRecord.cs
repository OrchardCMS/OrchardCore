using Newtonsoft.Json.Linq;
using Orchard.Data.Conventions;

namespace Orchard.Core.Settings.Metadata.Records
{
    public class ContentPartFieldDefinitionRecord
    {
        public virtual int Id { get; set; }
        public virtual ContentFieldDefinitionRecord ContentFieldDefinitionRecord { get; set; }
        public virtual string Name { get; set; }
        public virtual JObject Settings { get; set; }
    }
}