using System.Collections.Generic;

namespace OrchardCore.Data.Migration.Records
{
    public class DataMigrationRecord
    {
        public DataMigrationRecord()
        {
            DataMigrations = new List<DataMigration>();
        }
        public int Id { get; set; }
        public List<DataMigration> DataMigrations { get; set; }
    }

    public class DataMigration
    {
        public string DataMigrationClass { get; set; }
        public int? Version { get; set; }
    }
}