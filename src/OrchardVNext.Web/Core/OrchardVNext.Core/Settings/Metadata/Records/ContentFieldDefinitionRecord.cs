using OrchardVNext.ContentManagement;
using OrchardVNext.ContentManagement.Records;
using OrchardVNext.Data;

namespace OrchardVNext.Core.Settings.Metadata.Records {
    [Persistent]
    public class ContentFieldDefinitionRecord : DocumentRecord {
        public string Name {
            get { return this.RetrieveValue(x => x.Name); }
            set { this.StoreValue(x => x.Name, value); }
        }
    }
}
