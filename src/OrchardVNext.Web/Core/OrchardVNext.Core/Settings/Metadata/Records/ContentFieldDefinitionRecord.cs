using OrchardVNext.ContentManagement;
using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.Core.Settings.Metadata.Records {
    public class ContentFieldDefinitionRecord : DocumentRecord {
        public string Name {
            get { return this.RetrieveValue(x => x.Name); }
            set { this.StoreValue(x => x.Name, value); }
        }
    }
}
