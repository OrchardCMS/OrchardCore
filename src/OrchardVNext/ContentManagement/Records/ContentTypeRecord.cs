using OrchardVNext.Data;

namespace OrchardVNext.ContentManagement.Records {
    [Persistent]
    public class ContentTypeRecord : DocumentRecord {
        public string Name {
            get { return this.RetrieveValue(x => x.Name); }
            set { this.StoreValue(x => x.Name, value); }
        }
    }
}