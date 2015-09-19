using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Settings.Descriptor.Records {
    public class ShellFeatureRecord : DocumentRecord {
        public ShellDescriptorRecord ShellDescriptorRecord {
            get { return this.RetrieveValue(x => x.ShellDescriptorRecord); }
            set { this.StoreValue(x => x.ShellDescriptorRecord, value); }
        }

        public string Name {
            get { return this.RetrieveValue(x => x.Name); }
            set { this.StoreValue(x => x.Name, value); }
        }
    }
}