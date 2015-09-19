using System.Collections.Generic;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement;

namespace Orchard.Core.Settings.Descriptor.Records {
    public class ShellDescriptorRecord : DocumentRecord {
        public ShellDescriptorRecord() {
            Features=new List<ShellFeatureRecord>();
            Parameters=new List<ShellParameterRecord>();
        }

        public int SerialNumber {
            get { return this.RetrieveValue(x => x.SerialNumber); }
            set { this.StoreValue(x => x.SerialNumber, value); }
        }

        //[CascadeAllDeleteOrphan]
        public virtual IList<ShellFeatureRecord> Features { get; set; }
        
        //[CascadeAllDeleteOrphan]
        public virtual IList<ShellParameterRecord> Parameters { get; set; }
    }
}
