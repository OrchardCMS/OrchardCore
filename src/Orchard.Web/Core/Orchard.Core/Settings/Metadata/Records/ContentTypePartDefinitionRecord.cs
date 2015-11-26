using Newtonsoft.Json.Linq;
using Orchard.Data.Conventions;

namespace Orchard.Core.Settings.Metadata.Records
{
    public class ContentTypePartDefinitionRecord
    {
        public virtual int Id { get; set; }
        public virtual ContentPartDefinitionRecord ContentPartDefinitionRecord { get; set; }
        public virtual JObject Settings { get; set; }
    }
}