using Newtonsoft.Json;
using System.Collections.Generic;

namespace Orchard.Data.Migration.Records {
    public class DataMigrationRecord {
        public List<DataMigration> DataMigrations = new List<DataMigration>();
    }

    public class DataMigration {
        public virtual string DataMigrationClass { get; set; }
        public virtual int? Version { get; set; }
    }
}