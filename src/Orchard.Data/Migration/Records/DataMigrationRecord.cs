using Newtonsoft.Json;
using System.Collections.Generic;

namespace Orchard.Data.Migration.Records {
    public class DataMigrationDocument : StorageDocument {
        public List<DataMigrationRecord> DataMigrationRecords = new List<DataMigrationRecord>();

        public override string Data {
            get {
                return JsonConvert.SerializeObject(DataMigrationRecords);
            }

            set {
                DataMigrationRecords = JsonConvert.DeserializeObject<List<DataMigrationRecord>>(value);
            }
        }
    }

    public class DataMigrationRecord {
        public virtual string DataMigrationClass { get; set; }
        public virtual int? Version { get; set; }
    }
}