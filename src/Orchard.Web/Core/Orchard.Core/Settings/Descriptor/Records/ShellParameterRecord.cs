using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Settings.Descriptor.Records {
    public class ShellParameterRecord : DocumentRecord {
        public ShellDescriptorRecord ShellDescriptorRecord {
            get { return this.RetrieveValue(x => x.ShellDescriptorRecord); }
            set { this.StoreValue(x => x.ShellDescriptorRecord, value); }
        }
        public string Component {
            get { return this.RetrieveValue(x => x.Component); }
            set { this.StoreValue(x => x.Component, value); }
        }
        public string Name {
            get { return this.RetrieveValue(x => x.Name); }
            set { this.StoreValue(x => x.Name, value); }
        }
        public string Value {
            get { return this.RetrieveValue(x => x.Value); }
            set { this.StoreValue(x => x.Value, value); }
        }
    }
}